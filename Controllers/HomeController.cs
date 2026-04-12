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

        public HomeController(
            ILogger<HomeController> logger,
            IBarbeiroService barbeiroService,
            IClienteService clienteService,
            IAgendamentoService agendamentoService,
            ILoginService loginService)
        {
            _logger = logger;
            _barbeiroService = barbeiroService;
            _clienteService = clienteService;
            _agendamentoService = agendamentoService;
            _loginService = loginService;
        }

        public IActionResult Index()
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

            return View(new HomeAgendamentoViewModel
            {
                Agendamento = new AgendamentoModel
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
            });
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
        public IActionResult HomeAdministrador(int idBarbeiro)
        {
            var login = _loginService.ObterPorBarbeiroId(idBarbeiro);
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
        public IActionResult HomeFuncionario(int idBarbeiro)
        {
            var login = _loginService.ObterPorBarbeiroId(idBarbeiro);
            if (login?.TipoAcessoEnum != TipoAcessoEnum.Funcionario)
            {
                return Forbid();
            }

            var funcionario = _barbeiroService.ObterPorId(idBarbeiro);
            if (funcionario is null)
            {
                return NotFound();
            }

            var vm = new HomeFuncionarioViewModel
            {
                IdBarbeiro = funcionario.Id,
                NomeFuncionario = funcionario.NomeCompleto,
                Procedimentos = funcionario.Procedimentos,
                Agendamentos = _agendamentoService.ObterPorBarbeiroId(idBarbeiro, TipoAcessoEnum.Funcionario),
                Clientes = _clienteService.ObterPorBarbeiro(idBarbeiro, TipoAcessoEnum.Funcionario)
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult HomeCliente(int idCliente)
        {
            var login = _loginService.ObterPorClienteId(idCliente);
            if (login?.TipoAcessoEnum != TipoAcessoEnum.Cliente)
            {
                return Forbid();
            }

            var cliente = _clienteService.ObterPerfilCliente(idCliente, TipoAcessoEnum.Cliente);
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
