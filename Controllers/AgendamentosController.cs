using Microsoft.AspNetCore.Mvc;
using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Models.ViewModels;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Controllers
{
    public class AgendamentosController : Controller
    {
        private readonly IAgendamentoService _agendamentoService;
        private readonly IBarbeiroService _barbeiroService;
        private readonly IClienteService _clienteService;
        private readonly IProcedimentoService _procedimentoService;
        private readonly IUsuarioContextoService _usuarioContextoService;

        public AgendamentosController(
            IAgendamentoService agendamentoService,
            IBarbeiroService barbeiroService,
            IClienteService clienteService,
            IProcedimentoService procedimentoService,
            IUsuarioContextoService usuarioContextoService)
        {
            _agendamentoService = agendamentoService;
            _barbeiroService = barbeiroService;
            _clienteService = clienteService;
            _procedimentoService = procedimentoService;
            _usuarioContextoService = usuarioContextoService;
        }

        public IActionResult Index()
        {
            // Regra de leitura por perfil: a mesma pagina Index muda apenas a fonte dos dados.
            // SRP: a action orquestra e delega regra de consulta para os services.
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            if (tipoAcesso == TipoAcessoEnum.Administrador)
            {
                return View(ObterViewModel(new AgendamentoModel(), _agendamentoService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)));
            }

            if (tipoAcesso == TipoAcessoEnum.Funcionario)
            {
                var idBarbeiro = _usuarioContextoService.ObterIdBarbeiro();
                if (!idBarbeiro.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                return View(ObterViewModel(new AgendamentoModel(), _agendamentoService.ObterPorBarbeiroId(idBarbeiro.Value, TipoAcessoEnum.Funcionario)));
            }

            if (tipoAcesso == TipoAcessoEnum.Cliente)
            {
                var idCliente = _usuarioContextoService.ObterIdCliente();
                if (!idCliente.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var cliente = _clienteService.ObterPerfilCliente(idCliente.Value, TipoAcessoEnum.Cliente);
                return View(ObterViewModel(new AgendamentoModel(), _agendamentoService.ObterPorCpfCliente(cliente.CPF, TipoAcessoEnum.Cliente)));
            }

            return RedirectToAction("Login", "Auth");
        }

        public IActionResult Details(int id)
        {
            return RedirectToAction("ResumoAgendamento", "Home", new { idAgendamento = id });
        }

        public IActionResult Create()
        {
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            if (!tipoAcesso.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (tipoAcesso.Value is not (TipoAcessoEnum.Administrador or TipoAcessoEnum.Funcionario))
            {
                return Forbid();
            }

            var viewModel = ObterViewModelParaCreate(new AgendamentoModel(), tipoAcesso.Value);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AgendamentoCrudViewModel viewModel)
        {
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            if (!tipoAcesso.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (tipoAcesso.Value is not (TipoAcessoEnum.Administrador or TipoAcessoEnum.Funcionario))
            {
                return Forbid();
            }

            if (tipoAcesso.Value == TipoAcessoEnum.Funcionario)
            {
                var idBarbeiroLogado = _usuarioContextoService.ObterIdBarbeiro();
                if (!idBarbeiroLogado.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Funcionario sempre cria agendamento para si mesmo.
                viewModel.Agendamento.Barbeiro.Id = idBarbeiroLogado.Value;
                // Regra de negocio: no cadastro interno feito pelo barbeiro o status inicial e Agendado.
                viewModel.Agendamento.StatusAgendamentoEnum = StatusAgendamentoEnum.Agendado;
            }

            if (!ModelState.IsValid)
            {
                return View(ObterViewModelParaCreate(viewModel.Agendamento, tipoAcesso.Value));
            }

            _agendamentoService.Criar(viewModel.Agendamento);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var agendamento = _agendamentoService.ObterPorId(id, 0, TipoAcessoEnum.Administrador);
            return agendamento is null ? NotFound() : View(ObterViewModel(agendamento, _agendamentoService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, AgendamentoCrudViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(ObterViewModel(viewModel.Agendamento, _agendamentoService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)));
            }

            _agendamentoService.AtualizarDoFuncionario(id, viewModel.Agendamento.Barbeiro.Id, viewModel.Agendamento, TipoAcessoEnum.Funcionario);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AtualizarStatus(int idAgendamento, StatusAgendamentoEnum status)
        {
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            if (!tipoAcesso.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                if (tipoAcesso.Value == TipoAcessoEnum.Funcionario)
                {
                    var idBarbeiro = _usuarioContextoService.ObterIdBarbeiro();
                    if (!idBarbeiro.HasValue)
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    _agendamentoService.AtualizarStatus(idAgendamento, status, TipoAcessoEnum.Funcionario, idBarbeiro.Value);
                }
                else if (tipoAcesso.Value == TipoAcessoEnum.Cliente)
                {
                    var idCliente = _usuarioContextoService.ObterIdCliente();
                    if (!idCliente.HasValue)
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    var cliente = _clienteService.ObterPerfilCliente(idCliente.Value, TipoAcessoEnum.Cliente);
                    _agendamentoService.AtualizarStatus(idAgendamento, status, TipoAcessoEnum.Cliente, cpfClienteSolicitante: cliente.CPF);
                }
                else if (tipoAcesso.Value == TipoAcessoEnum.Administrador)
                {
                    _agendamentoService.AtualizarStatus(idAgendamento, status, TipoAcessoEnum.Administrador);
                }
                else
                {
                    return Forbid();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Delete(int id)
        {
            var agendamento = _agendamentoService.ObterPorId(id, 0, TipoAcessoEnum.Administrador);
            return agendamento is null ? NotFound() : View(ObterViewModel(agendamento, _agendamentoService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id, int barbeiroId)
        {
            _agendamentoService.Excluir(id, barbeiroId, TipoAcessoEnum.Funcionario);
            return RedirectToAction(nameof(Index));
        }

        private AgendamentoCrudViewModel ObterViewModel(AgendamentoModel agendamento, List<AgendamentoModel> agendamentos)
        {
            var barbeiros = _barbeiroService.ObterTodos();
            var clientes = _clienteService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador);
            var procedimentos = _procedimentoService.ObterTodos();

            return new AgendamentoCrudViewModel
            {
                Agendamento = agendamento,
                Agendamentos = agendamentos,
                Barbeiros = barbeiros,
                Clientes = clientes,
                Procedimentos = procedimentos,
                PrecosPorBarbeiroProcedimento = barbeiros.ToDictionary(
                    x => x.Id,
                    x => x.Procedimentos.ToDictionary(p => (int)p.ProcedimentoEnum, p => p.PrecoPorBarbeiro))
            };
        }

        private AgendamentoCrudViewModel ObterViewModelParaCreate(AgendamentoModel agendamento, TipoAcessoEnum tipoAcesso)
        {
            var procedimentos = _procedimentoService.ObterTodos();
            List<BarbeiroModel> barbeiros;
            List<ClienteModel> clientes;

            if (tipoAcesso == TipoAcessoEnum.Funcionario)
            {
                var idBarbeiroLogado = _usuarioContextoService.ObterIdBarbeiro();
                if (!idBarbeiroLogado.HasValue)
                {
                    throw new UnauthorizedAccessException("Funcionario sem id de barbeiro na sessao.");
                }

                var barbeiro = _barbeiroService.ObterPorId(idBarbeiroLogado.Value)
                    ?? throw new KeyNotFoundException($"Barbeiro {idBarbeiroLogado.Value} nao encontrado.");

                barbeiros = [barbeiro];
                clientes = _clienteService.ObterPorBarbeiro(idBarbeiroLogado.Value, TipoAcessoEnum.Funcionario)
                    .Where(x => x.Ativo)
                    .ToList();

                agendamento.Barbeiro = barbeiro;
                // Valor inicial da tela para refletir a regra do fluxo de cadastro pelo funcionario.
                agendamento.StatusAgendamentoEnum = StatusAgendamentoEnum.Agendado;
            }
            else
            {
                barbeiros = _barbeiroService.ObterTodos()
                    .Where(x => x.TipoAcesso is TipoAcessoEnum.Administrador or TipoAcessoEnum.Funcionario)
                    .ToList();

                clientes = _clienteService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)
                    .Where(x => x.Ativo)
                    .ToList();
            }

            ViewBag.PodeEscolherBarbeiro = tipoAcesso == TipoAcessoEnum.Administrador;

            return new AgendamentoCrudViewModel
            {
                Agendamento = agendamento,
                Agendamentos = [],
                Barbeiros = barbeiros,
                Clientes = clientes,
                Procedimentos = procedimentos,
                PrecosPorBarbeiroProcedimento = barbeiros.ToDictionary(
                    x => x.Id,
                    x => x.Procedimentos.ToDictionary(p => (int)p.ProcedimentoEnum, p => p.PrecoPorBarbeiro))
            };
        }
    }
}