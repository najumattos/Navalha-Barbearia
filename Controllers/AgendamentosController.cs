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

        public AgendamentosController(
            IAgendamentoService agendamentoService,
            IBarbeiroService barbeiroService,
            IClienteService clienteService,
            IProcedimentoService procedimentoService)
        {
            _agendamentoService = agendamentoService;
            _barbeiroService = barbeiroService;
            _clienteService = clienteService;
            _procedimentoService = procedimentoService;
        }

        public IActionResult Index()
        {
            return View(ObterViewModel(new AgendamentoModel()));
        }

        public IActionResult Details(int id)
        {
            var agendamento = _agendamentoService.ObterPorId(id, 0, TipoAcessoEnum.Administrador);
            return agendamento is null ? NotFound() : View(agendamento);
        }

        public IActionResult Create()
        {
            return View(ObterViewModel(new AgendamentoModel()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AgendamentoCrudViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(ObterViewModel(viewModel.Agendamento));
            }

            _agendamentoService.Criar(viewModel.Agendamento);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var agendamento = _agendamentoService.ObterPorId(id, 0, TipoAcessoEnum.Administrador);
            return agendamento is null ? NotFound() : View(ObterViewModel(agendamento));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, AgendamentoCrudViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(ObterViewModel(viewModel.Agendamento));
            }

            _agendamentoService.AtualizarDoFuncionario(id, viewModel.Agendamento.Barbeiro.Id, viewModel.Agendamento, TipoAcessoEnum.Funcionario);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var agendamento = _agendamentoService.ObterPorId(id, 0, TipoAcessoEnum.Administrador);
            return agendamento is null ? NotFound() : View(ObterViewModel(agendamento));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id, int barbeiroId)
        {
            _agendamentoService.Excluir(id, barbeiroId, TipoAcessoEnum.Funcionario);
            return RedirectToAction(nameof(Index));
        }

        private AgendamentoCrudViewModel ObterViewModel(AgendamentoModel agendamento)
        {
            var barbeiros = _barbeiroService.ObterTodos();
            var clientes = _clienteService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador);
            var procedimentos = _procedimentoService.ObterTodos();

            return new AgendamentoCrudViewModel
            {
                Agendamento = agendamento,
                Agendamentos = _agendamentoService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador),
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