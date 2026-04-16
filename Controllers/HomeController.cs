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
        private const string ResumoAgendamentoViewPath = "~/Views/Agendamentos/ResumoAgendamento.cshtml";

        private readonly ILogger<HomeController> _logger;
        private readonly IBarbeiroService _barbeiroService;
        private readonly IClienteService _clienteService;
        private readonly IAgendamentoService _agendamentoService;
        private readonly IProcedimentoService _procedimentoService;
        private readonly ISlotHorarioService _slotHorarioService;
        private readonly ILoginService _loginService;
        private readonly IUsuarioContextoService _usuarioContextoService;

        public HomeController(
            ILogger<HomeController> logger,
            IBarbeiroService barbeiroService,
            IClienteService clienteService,
            IAgendamentoService agendamentoService,
            IProcedimentoService procedimentoService,
            ISlotHorarioService slotHorarioService,
            ILoginService loginService,
            IUsuarioContextoService usuarioContextoService)
        {
            _logger = logger;
            _barbeiroService = barbeiroService;
            _clienteService = clienteService;
            _agendamentoService = agendamentoService;
            _procedimentoService = procedimentoService;
            _slotHorarioService = slotHorarioService;
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
                return View("Index", CriarHomeAgendamentoViewModel(viewModel.Agendamento, viewModel.DataSelecionada));
            }

            try
            {
                var resumoViewModel = CriarResumoAgendamentoViewModel(viewModel.Agendamento, exibirBotaoConfirmar: true);
                return View(ResumoAgendamentoViewPath, resumoViewModel);
            }
            catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Index", CriarHomeAgendamentoViewModel(viewModel.Agendamento, viewModel.DataSelecionada));
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
            catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException or InvalidOperationException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                var resumoViewModel = CriarResumoAgendamentoViewModel(viewModel.AgendamentoAtual, exibirBotaoConfirmar: true);
                return View(ResumoAgendamentoViewPath, resumoViewModel);
            }
        }

        [HttpGet]
        public IActionResult ResumoAgendamento(int idAgendamento, string? origem = null)
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

            return View(ResumoAgendamentoViewPath, new HomeResumoAgendamentoViewModel
            {
                Cliente = cliente,
                AgendamentoAtual = agendamentoAtual,
                HistoricoRecente = historicoRecente,
                ExibirBotaoConfirmar = false,
                Origem = origem
            });
        }

        private HomeResumoAgendamentoViewModel CriarResumoAgendamentoViewModel(AgendamentoModel agendamentoAtual, bool exibirBotaoConfirmar)
        {
            if (agendamentoAtual.SlotHorarioId <= 0)
            {
                throw new ArgumentException("Selecione um slot de horario para continuar.");
            }

            var slotSelecionado = _slotHorarioService.ObterPorId(agendamentoAtual.SlotHorarioId)
                ?? throw new KeyNotFoundException($"Slot de horario {agendamentoAtual.SlotHorarioId} nao encontrado.");

            var barbeiro = _barbeiroService.ObterPorId(agendamentoAtual.Barbeiro.Id)
                ?? throw new KeyNotFoundException("Barbeiro nao encontrado para o agendamento.");

            if (slotSelecionado.BarbeiroId != barbeiro.Id)
            {
                throw new ArgumentException("O slot selecionado nao pertence ao barbeiro informado.");
            }

            var cliente = _clienteService.ObterPorCpfPublico(agendamentoAtual.Cliente.CPF)
                ?? throw new KeyNotFoundException("Cliente nao encontrado para o CPF informado.");

            var historicoRecente = _agendamentoService.ObterPorCpfCliente(cliente.CPF, TipoAcessoEnum.Cliente)
                .OrderByDescending(x => x.DataHora)
                .Take(2)
                .ToList();

            if (!ProcedimentoDisponivelParaBarbeiro(barbeiro, (int)agendamentoAtual.Procedimento))
            {
                throw new ArgumentException("O procedimento selecionado nao esta ativo para o barbeiro informado.");
            }

            agendamentoAtual.Barbeiro = barbeiro;
            agendamentoAtual.Cliente = cliente;
            agendamentoAtual.DataHora = slotSelecionado.Inicio;
            agendamentoAtual.Preco = ObterPrecoPorBarbeiro(barbeiro, (int)agendamentoAtual.Procedimento);

            return new HomeResumoAgendamentoViewModel
            {
                Cliente = cliente,
                AgendamentoAtual = agendamentoAtual,
                HistoricoRecente = historicoRecente,
                ExibirBotaoConfirmar = exibirBotaoConfirmar
            };
        }

        private HomeAgendamentoViewModel CriarHomeAgendamentoViewModel(AgendamentoModel? agendamentoBase = null, DateTime? dataSelecionada = null)
        {
            // A Home apresenta o formulario inicial sem acoplar regra de negocio de envio.
            var barbeiros = _barbeiroService.ObterTodos()
                .Where(x => x.TipoAcesso is TipoAcessoEnum.Administrador or TipoAcessoEnum.Funcionario)
                .ToList();

            var dataBase = dataSelecionada?.Date
                ?? (agendamentoBase is not null && agendamentoBase.DataHora.Date != default
                    ? agendamentoBase.DataHora.Date
                    : DateTime.Today.AddDays(1));

            var barbeiroPadrao = barbeiros.FirstOrDefault() ?? new BarbeiroModel();
            var procedimentoPadraoId = ObterProcedimentoPadraoId(barbeiroPadrao);
            var procedimentoPadrao = (ProcedimentoEnum)procedimentoPadraoId;
            var precoPadrao = ObterPrecoPorBarbeiro(barbeiroPadrao, procedimentoPadraoId);

            var mapaPrecos = barbeiros.ToDictionary(x => x.Id, MontarMapaPrecosPorProcedimento);

            if (agendamentoBase is not null && agendamentoBase.Barbeiro.Id > 0)
            {
                var barbeiroSelecionado = barbeiros.FirstOrDefault(x => x.Id == agendamentoBase.Barbeiro.Id);
                if (barbeiroSelecionado is not null)
                {
                    agendamentoBase.Barbeiro = barbeiroSelecionado;
                    agendamentoBase.Preco = ObterPrecoPorBarbeiro(barbeiroSelecionado, (int)agendamentoBase.Procedimento);
                }
            }

            var barbeiroSelecionadoId = agendamentoBase?.Barbeiro.Id > 0
                ? agendamentoBase.Barbeiro.Id
                : barbeiroPadrao.Id;

            var barbeiroSelecionadoParaTela = barbeiros.FirstOrDefault(x => x.Id == barbeiroSelecionadoId) ?? barbeiroPadrao;
            var procedimentosDisponiveis = ObterProcedimentosDisponiveisDoBarbeiro(barbeiroSelecionadoParaTela);

            var slotsDisponiveis = barbeiroSelecionadoId > 0
                ? _slotHorarioService.ObterSlotsDisponiveis(barbeiroSelecionadoId, dataBase)
                : [];

            return new HomeAgendamentoViewModel
            {
                Agendamento = agendamentoBase ?? new AgendamentoModel
                {
                    StatusAgendamentoEnum = StatusAgendamentoEnum.Pendente,
                    Barbeiro = barbeiroPadrao,
                    Cliente = new ClienteModel(),
                    Procedimento = procedimentoPadrao,
                    Preco = precoPadrao
                },
                Barbeiros = barbeiros,
                Procedimentos = procedimentosDisponiveis,
                DataSelecionada = dataBase,
                SlotsDisponiveis = slotsDisponiveis,
                PrecosPorBarbeiroProcedimento = mapaPrecos
            };
        }

        private static decimal ObterPrecoPorBarbeiro(BarbeiroModel barbeiro, int procedimentoId)
        {
            // Prioriza a nova relacao N:N; fallback preserva compatibilidade com estrutura legada.
            var possuiRelacoes = barbeiro.RelacoesProcedimentos.Any();

            var relacaoAtiva = barbeiro.RelacoesProcedimentos
                .FirstOrDefault(x => x.ProcedimentoId == procedimentoId && x.Ativo);

            if (relacaoAtiva is not null)
            {
                return relacaoAtiva.PrecoPorBarbeiro;
            }

            if (possuiRelacoes)
            {
                return 0m;
            }

            var procedimentoDoBarbeiro = barbeiro.Procedimentos.FirstOrDefault(x => x.Id == procedimentoId);
            return procedimentoDoBarbeiro?.PrecoPorBarbeiro ?? 0m;
        }

        private static int ObterProcedimentoPadraoId(BarbeiroModel barbeiro)
        {
            var possuiRelacoes = barbeiro.RelacoesProcedimentos.Any();

            var procedimentoIdDaRelacao = barbeiro.RelacoesProcedimentos
                .Where(x => x.Ativo)
                .Select(x => x.ProcedimentoId)
                .FirstOrDefault();

            if (procedimentoIdDaRelacao > 0)
            {
                return procedimentoIdDaRelacao;
            }

            if (possuiRelacoes)
            {
                return 0;
            }

            return barbeiro.Procedimentos.FirstOrDefault()?.Id ?? 1;
        }

        private static Dictionary<int, decimal> MontarMapaPrecosPorProcedimento(BarbeiroModel barbeiro)
        {
            var mapa = new Dictionary<int, decimal>();

            var possuiRelacoes = barbeiro.RelacoesProcedimentos.Any();

            foreach (var relacao in barbeiro.RelacoesProcedimentos.Where(x => x.Ativo))
            {
                mapa[relacao.ProcedimentoId] = relacao.PrecoPorBarbeiro;
            }

            if (possuiRelacoes)
            {
                return mapa;
            }

            foreach (var procedimento in barbeiro.Procedimentos)
            {
                if (!mapa.ContainsKey(procedimento.Id))
                {
                    mapa[procedimento.Id] = procedimento.PrecoPorBarbeiro;
                }
            }

            return mapa;
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

            return View(CriarHomeFuncionarioViewModel(funcionario.Id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AtualizarPrecoPorBarbeiroFuncionario(int procedimentoId, decimal precoPorBarbeiro)
        {
            var idBarbeiro = _usuarioContextoService.ObterIdBarbeiro();
            if (!idBarbeiro.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var login = _loginService.ObterPorBarbeiroId(idBarbeiro.Value);
            if (login?.TipoAcessoEnum != TipoAcessoEnum.Funcionario)
            {
                return Forbid();
            }

            try
            {
                _barbeiroService.AtualizarPrecoPorBarbeiro(idBarbeiro.Value, procedimentoId, precoPorBarbeiro, TipoAcessoEnum.Funcionario);
                return RedirectToAction(nameof(HomeFuncionario), new { idBarbeiro = idBarbeiro.Value });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(nameof(HomeFuncionario), CriarHomeFuncionarioViewModel(idBarbeiro.Value));
            }
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

        [HttpGet]
        public IActionResult BuscarSlotsDisponiveis(int barbeiroId, DateTime data)
        {
            if (barbeiroId <= 0)
            {
                return Json(new List<object>());
            }

            var slots = _slotHorarioService.ObterSlotsDisponiveis(barbeiroId, data.Date)
                .Where(x => x.StatusHorarioEnum == StatusHorarioEnum.Livre)
                .Select(x => new
                {
                    id = x.Id,
                    inicio = x.Inicio,
                    inicioFormatado = x.Inicio.ToString("HH:mm"),
                    fimFormatado = x.Fim.ToString("HH:mm")
                })
                .ToList();

            return Json(slots);
        }

        [HttpGet]
        public IActionResult BuscarProcedimentosPorBarbeiro(int barbeiroId)
        {
            if (barbeiroId <= 0)
            {
                return Json(new List<object>());
            }

            var barbeiro = _barbeiroService.ObterPorId(barbeiroId);
            if (barbeiro is null)
            {
                return Json(new List<object>());
            }

            var mapaPrecos = MontarMapaPrecosPorProcedimento(barbeiro);
            var procedimentos = ObterProcedimentosDisponiveisDoBarbeiro(barbeiro)
                .Select(procedimento => new
                {
                    id = procedimento.Id,
                    nome = procedimento.Nome,
                    precoPorBarbeiro = mapaPrecos.TryGetValue(procedimento.Id, out var preco) ? preco : procedimento.PrecoPorBarbeiro
                })
                .ToList();

            return Json(procedimentos);
        }

        private List<ProcedimentoModel> ObterProcedimentosDisponiveisDoBarbeiro(BarbeiroModel barbeiro)
        {
            var possuiRelacoes = barbeiro.RelacoesProcedimentos.Any();

            if (possuiRelacoes)
            {
                return barbeiro.RelacoesProcedimentos
                    .Where(x => x.Ativo)
                    .Select(x => _procedimentoService.ObterPorId(x.ProcedimentoId))
                    .Where(x => x is not null)
                    .Select(x => x!)
                    .GroupBy(x => x.Id)
                    .Select(x => x.First())
                    .OrderBy(x => x.Nome)
                    .ToList();
            }

            return barbeiro.Procedimentos
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .OrderBy(x => x.Nome)
                .ToList();
        }

        private static bool ProcedimentoDisponivelParaBarbeiro(BarbeiroModel barbeiro, int procedimentoId)
        {
            if (procedimentoId <= 0)
            {
                return false;
            }

            var possuiRelacoes = barbeiro.RelacoesProcedimentos.Any();
            if (possuiRelacoes)
            {
                return barbeiro.RelacoesProcedimentos.Any(x => x.ProcedimentoId == procedimentoId && x.Ativo);
            }

            return barbeiro.Procedimentos.Any(x => x.Id == procedimentoId);
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private HomeFuncionarioViewModel CriarHomeFuncionarioViewModel(int idBarbeiro)
        {
            var funcionario = _barbeiroService.ObterPorId(idBarbeiro)
                ?? throw new KeyNotFoundException("Barbeiro nao encontrado para montar o painel do funcionario.");

            var procedimentosDoBarbeiro = MontarProcedimentosDoBarbeiro(funcionario);

            return new HomeFuncionarioViewModel
            {
                IdBarbeiro = funcionario.Id,
                NomeFuncionario = funcionario.NomeCompleto,
                ProcedimentosDoBarbeiro = procedimentosDoBarbeiro,
                Agendamentos = _agendamentoService.ObterPorBarbeiroId(idBarbeiro, TipoAcessoEnum.Funcionario),
                Clientes = _clienteService.ObterPorBarbeiro(idBarbeiro, TipoAcessoEnum.Funcionario)
            };
        }

        private List<ProcedimentoDoBarbeiroViewModel> MontarProcedimentosDoBarbeiro(BarbeiroModel barbeiro)
        {
            var catalogo = _procedimentoService.ObterTodos().ToDictionary(x => x.Id, x => x);
            var procedimentos = new List<ProcedimentoDoBarbeiroViewModel>();

            foreach (var relacao in barbeiro.RelacoesProcedimentos.Where(x => x.Ativo))
            {
                catalogo.TryGetValue(relacao.ProcedimentoId, out var procedimentoCatalogo);
                var procedimentoLegado = barbeiro.Procedimentos.FirstOrDefault(x => x.Id == relacao.ProcedimentoId);

                if (procedimentoCatalogo is null && procedimentoLegado is null)
                {
                    continue;
                }

                procedimentos.Add(new ProcedimentoDoBarbeiroViewModel
                {
                    Id = relacao.ProcedimentoId,
                    Nome = procedimentoCatalogo?.Nome ?? procedimentoLegado?.Nome ?? string.Empty,
                    Descricao = procedimentoCatalogo?.Descricao ?? procedimentoLegado?.Descricao ?? string.Empty,
                    PrecoBase = procedimentoCatalogo?.PrecoBase ?? procedimentoLegado?.PrecoBase ?? 0m,
                    PrecoPorBarbeiro = relacao.PrecoPorBarbeiro,
                    Ativo = relacao.Ativo
                });
            }

            foreach (var procedimentoLegado in barbeiro.Procedimentos)
            {
                if (procedimentos.Any(x => x.Id == procedimentoLegado.Id))
                {
                    continue;
                }

                procedimentos.Add(new ProcedimentoDoBarbeiroViewModel
                {
                    Id = procedimentoLegado.Id,
                    Nome = procedimentoLegado.Nome,
                    Descricao = procedimentoLegado.Descricao,
                    PrecoBase = procedimentoLegado.PrecoBase,
                    PrecoPorBarbeiro = procedimentoLegado.PrecoPorBarbeiro,
                    Ativo = true
                });
            }

            return procedimentos.OrderBy(x => x.Nome).ToList();
        }
    }
}
