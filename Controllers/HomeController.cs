using Microsoft.AspNetCore.Mvc;
using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Models.ViewModels;
using Navalha_Barbearia.Services.Interfaces;
using System.Diagnostics;

namespace Navalha_Barbearia.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBarbeiroService _barbeiroService;
        private readonly IClienteService _clienteService;
        private readonly IAgendamentoService _agendamentoService;
        private readonly ILoginService _loginService;
        private readonly IUsuarioContextoService _usuarioContextoService;

        public HomeController(
            ILogger<HomeController> logger,
            IBarbeiroService barbeiroService,
            IClienteService clienteService,
            IAgendamentoService agendamentoService,
            ILoginService loginService,
            IUsuarioContextoService usuarioContextoService)
        {
            _logger = logger;
            _barbeiroService = barbeiroService;
            _clienteService = clienteService;
            _agendamentoService = agendamentoService;
            _loginService = loginService;
            _usuarioContextoService = usuarioContextoService;
        }

        public IActionResult Index()
        {
            return View(CriarHomeAgendamentoViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Agendar(HomeAgendamentoViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", CriarHomeAgendamentoViewModel(viewModel.Agendamento));
            }

            try
            {
                var resumoViewModel = CriarResumoAgendamentoViewModel(viewModel.Agendamento, exibirBotaoConfirmar: true);
                return View(nameof(ResumoAgendamento), resumoViewModel);
            }
            catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Index", CriarHomeAgendamentoViewModel(viewModel.Agendamento));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarAgendamento(HomeResumoAgendamentoViewModel viewModel)
        {
            try
            {
                var agendamentoParaCriar = viewModel.AgendamentoAtual;
                agendamentoParaCriar.StatusAgendamentoEnum = StatusAgendamentoEnum.AguardandoConfirmacaoBarbeiro;

                var agendamentoCriado = _agendamentoService.Criar(agendamentoParaCriar);
                return RedirectToAction(nameof(ResumoAgendamento), new { idAgendamento = agendamentoCriado.IdAgendamento });
            }
            catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                var resumoViewModel = CriarResumoAgendamentoViewModel(viewModel.AgendamentoAtual, exibirBotaoConfirmar: true);
                return View(nameof(ResumoAgendamento), resumoViewModel);
            }
        }

        [HttpGet]
        public IActionResult ResumoAgendamento(int idAgendamento)
        {
            var agendamentoAtual = _agendamentoService.ObterPorId(idAgendamento, 0, TipoAcessoEnum.Administrador);
            if (agendamentoAtual is null)
            {
                return NotFound();
            }

            var cliente = _clienteService.ObterPorCpfPublico(agendamentoAtual.Cliente.CPF) ?? agendamentoAtual.Cliente;
            var historicoRecente = _agendamentoService.ObterPorCpfCliente(cliente.CPF, TipoAcessoEnum.Cliente)
                .Where(x => x.IdAgendamento != agendamentoAtual.IdAgendamento)
                .OrderByDescending(x => x.DataHora)
                .Take(2)
                .ToList();

            return View(new HomeResumoAgendamentoViewModel
            {
                Cliente = cliente,
                AgendamentoAtual = agendamentoAtual,
                HistoricoRecente = historicoRecente,
                ExibirBotaoConfirmar = false
            });
        }

        private HomeResumoAgendamentoViewModel CriarResumoAgendamentoViewModel(AgendamentoModel agendamentoAtual, bool exibirBotaoConfirmar)
        {
            var barbeiro = _barbeiroService.ObterPorId(agendamentoAtual.Barbeiro.Id)
                ?? throw new KeyNotFoundException("Barbeiro nao encontrado para o agendamento.");

            var cliente = _clienteService.ObterPorCpfPublico(agendamentoAtual.Cliente.CPF)
                ?? throw new KeyNotFoundException("Cliente nao encontrado para o CPF informado.");

            var historicoRecente = _agendamentoService.ObterPorCpfCliente(cliente.CPF, TipoAcessoEnum.Cliente)
                .OrderByDescending(x => x.DataHora)
                .Take(2)
                .ToList();

            agendamentoAtual.Barbeiro = barbeiro;
            agendamentoAtual.Cliente = cliente;
            agendamentoAtual.Preco = ObterPrecoPorBarbeiro(barbeiro, agendamentoAtual.Procedimento);

            return new HomeResumoAgendamentoViewModel
            {
                Cliente = cliente,
                AgendamentoAtual = agendamentoAtual,
                HistoricoRecente = historicoRecente,
                ExibirBotaoConfirmar = exibirBotaoConfirmar
            };
        }

        private HomeAgendamentoViewModel CriarHomeAgendamentoViewModel(AgendamentoModel? agendamentoBase = null)
        {
            // A Home apresenta o formulario inicial sem acoplar regra de negocio de envio.
            var barbeiros = _barbeiroService.ObterTodos()
                .Where(x => x.TipoAcesso is TipoAcessoEnum.Administrador or TipoAcessoEnum.Funcionario)
                .ToList();

            var barbeiroPadrao = barbeiros.FirstOrDefault() ?? new BarbeiroModel();
            var procedimentoPadrao = barbeiroPadrao.Procedimentos.FirstOrDefault()?.ProcedimentoEnum ?? ProcedimentoEnum.Corte;
            var precoPadrao = ObterPrecoPorBarbeiro(barbeiroPadrao, procedimentoPadrao);

            var mapaPrecos = barbeiros.ToDictionary(
                x => x.Id,
                x => x.Procedimentos.ToDictionary(p => (int)p.ProcedimentoEnum, p => p.PrecoPorBarbeiro));

            if (agendamentoBase is not null && agendamentoBase.Barbeiro.Id > 0)
            {
                var barbeiroSelecionado = barbeiros.FirstOrDefault(x => x.Id == agendamentoBase.Barbeiro.Id);
                if (barbeiroSelecionado is not null)
                {
                    agendamentoBase.Barbeiro = barbeiroSelecionado;
                    agendamentoBase.Preco = ObterPrecoPorBarbeiro(barbeiroSelecionado, agendamentoBase.Procedimento);
                }
            }

            return new HomeAgendamentoViewModel
            {
                Agendamento = agendamentoBase ?? new AgendamentoModel
                {
                    DataHora = DateTime.Now.AddDays(1),
                    StatusAgendamentoEnum = StatusAgendamentoEnum.Pendente,
                    Barbeiro = barbeiroPadrao,
                    Cliente = new ClienteModel(),
                    Procedimento = procedimentoPadrao,
                    Preco = precoPadrao
                },
                Barbeiros = barbeiros,
                PrecosPorBarbeiroProcedimento = mapaPrecos
            };
        }

        private static decimal ObterPrecoPorBarbeiro(BarbeiroModel barbeiro, ProcedimentoEnum procedimento)
        {
            // Encapsular a regra em metodo privado melhora legibilidade e facilita manutencao.
            var procedimentoDoBarbeiro = barbeiro.Procedimentos.FirstOrDefault(x => x.ProcedimentoEnum == procedimento);
            return procedimentoDoBarbeiro?.PrecoPorBarbeiro ?? 0m;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult HomeAdministrador(int? idBarbeiro)
        {
            // Clean Code: fallback explicito para sessao evita duplicacao de links com query string.
            var idBarbeiroResolvido = idBarbeiro ?? _usuarioContextoService.ObterIdBarbeiro();
            if (!idBarbeiroResolvido.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var login = _loginService.ObterPorBarbeiroId(idBarbeiroResolvido.Value);
            if (login?.TipoAcessoEnum != TipoAcessoEnum.Administrador)
            {
                return Forbid();
            }

            // DIP e SRP: o controller usa service e apenas prepara dados para a view.
            var barbeiros = _barbeiroService.ObterTodos();
            var agendamentos = _agendamentoService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador);
            var clientes = _clienteService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador);

            return View(new HomeAdministradorViewModel
            {
                Barbeiros = barbeiros,
                Agendamentos = agendamentos,
                Clientes = clientes
            });
        }

        [HttpGet]
        public IActionResult HomeFuncionario(int? idBarbeiro)
        {
            var idBarbeiroResolvido = idBarbeiro ?? _usuarioContextoService.ObterIdBarbeiro();
            if (!idBarbeiroResolvido.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var login = _loginService.ObterPorBarbeiroId(idBarbeiroResolvido.Value);
            if (login?.TipoAcessoEnum != TipoAcessoEnum.Funcionario)
            {
                return Forbid();
            }

            var funcionario = _barbeiroService.ObterPorId(idBarbeiroResolvido.Value);
            if (funcionario is null)
            {
                return NotFound();
            }

            var vm = new HomeFuncionarioViewModel
            {
                IdBarbeiro = funcionario.Id,
                NomeFuncionario = funcionario.NomeCompleto,
                Procedimentos = funcionario.Procedimentos,
                Agendamentos = _agendamentoService.ObterPorBarbeiroId(idBarbeiroResolvido.Value, TipoAcessoEnum.Funcionario),
                Clientes = _clienteService.ObterPorBarbeiro(idBarbeiroResolvido.Value, TipoAcessoEnum.Funcionario)
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult HomeCliente(int? idCliente)
        {
            var idClienteResolvido = idCliente ?? _usuarioContextoService.ObterIdCliente();
            if (!idClienteResolvido.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var login = _loginService.ObterPorClienteId(idClienteResolvido.Value);
            if (login?.TipoAcessoEnum != TipoAcessoEnum.Cliente)
            {
                return Forbid();
            }

            var cliente = _clienteService.ObterPerfilCliente(idClienteResolvido.Value, TipoAcessoEnum.Cliente);
            var agendamentos = _agendamentoService.ObterPorCpfCliente(cliente.CPF, TipoAcessoEnum.Cliente);

            return View(new HomeClienteViewModel
            {
                Cliente = cliente,
                Agendamentos = agendamentos
            });
        }

        [HttpGet]
        public IActionResult BuscarClientePorCpf(string cpf)
        {
            // Endpoint publico de UX: auto preenche nome por CPF sem autenticar.
            var cliente = _clienteService.ObterPorCpfPublico(cpf);
            if (cliente is null)
            {
                return Json(new { encontrado = false });
            }

            return Json(new
            {
                encontrado = true,
                id = cliente.Id,
                nomeCompleto = cliente.NomeCompleto,
                cpf = cliente.CPF,
                telefone = cliente.Telefone
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AtualizarAgendamentoFuncionario(int idBarbeiro, int idAgendamento, AgendamentoModel agendamento)
        {
            var login = _loginService.ObterPorBarbeiroId(idBarbeiro);
            if (login?.TipoAcessoEnum != TipoAcessoEnum.Funcionario)
            {
                return Forbid();
            }

            try
            {
                // O controller permanece fino: valida autenticacao e delega a regra para o service.
                _agendamentoService.AtualizarDoFuncionario(idAgendamento, idBarbeiro, agendamento, TipoAcessoEnum.Funcionario);
                return RedirectToAction(nameof(HomeFuncionario), new { idBarbeiro });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction(nameof(HomeFuncionario), new { idBarbeiro });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AtualizarStatusAgendamentoCliente(int idCliente, int idAgendamento, StatusAgendamentoEnum status)
        {
            var login = _loginService.ObterPorClienteId(idCliente);
            if (login?.TipoAcessoEnum != TipoAcessoEnum.Cliente)
            {
                return Forbid();
            }

            try
            {
                var cliente = _clienteService.ObterPerfilCliente(idCliente, TipoAcessoEnum.Cliente);
                _agendamentoService.AtualizarStatusDoCliente(idAgendamento, cliente.CPF, status, TipoAcessoEnum.Cliente);
                return RedirectToAction(nameof(HomeCliente), new { idCliente });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction(nameof(HomeCliente), new { idCliente });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
