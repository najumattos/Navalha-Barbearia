using Microsoft.AspNetCore.Mvc;
using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Models.ViewModels;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Controllers;

public class AgendamentosController(
    IAgendamentoService agendamentoService,
    ISlotHorarioService slotHorarioService,
    IBarbeiroService barbeiroService,
    IClienteService clienteService,
    IProcedimentoService procedimentoService,
    IUsuarioContextoService usuarioContextoService) : Controller
{
    public IActionResult Index()
    {
        // Regra de leitura por perfil: a mesma pagina Index muda apenas a fonte dos dados.
        // SRP: a action orquestra e delega regra de consulta para os services.
        var tipoAcesso = usuarioContextoService.ObterTipoAcesso();
        if (tipoAcesso == TipoAcessoEnum.Administrador)
        {
            return View(ObterViewModel(new AgendamentoModel(), agendamentoService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)));
        }

        if (tipoAcesso == TipoAcessoEnum.Funcionario)
        {
            var idBarbeiro = usuarioContextoService.ObterIdBarbeiro();
            if (!idBarbeiro.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(ObterViewModel(new AgendamentoModel(), agendamentoService.ObterPorBarbeiroId(idBarbeiro.Value, TipoAcessoEnum.Funcionario)));
        }

        if (tipoAcesso == TipoAcessoEnum.Cliente)
        {
            var idCliente = usuarioContextoService.ObterIdCliente();
            if (!idCliente.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var cliente = clienteService.ObterPerfilCliente(idCliente.Value, TipoAcessoEnum.Cliente);
            var historico = agendamentoService.ObterPorCpfCliente(cliente.CPF, TipoAcessoEnum.Cliente)
                .OrderByDescending(x => x.DataHora)
                .ToList();

            return View(ObterViewModelSomenteHistoricoCliente(historico));
        }

        return RedirectToAction("Login", "Auth");
    }

    public IActionResult Details(int id)
    {
        if (!UsuarioEhClienteComAgendamentoVinculado(id))
        {
            return Forbid();
        }

        return RedirectToAction(nameof(ResumoAgendamento), new { idAgendamento = id });
    }

    [HttpGet]
    public IActionResult ResumoAgendamento(int idAgendamento)
    {
        if (!UsuarioEhClienteComAgendamentoVinculado(idAgendamento))
        {
            return Forbid();
        }

        return RedirectToAction(nameof(HomeController.ResumoAgendamento), "Home", new { idAgendamento, origem = "agendamentos" });
    }

    public IActionResult Create()
    {
        var tipoAcesso = usuarioContextoService.ObterTipoAcesso();
        if (!tipoAcesso.HasValue)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (tipoAcesso.Value is not (TipoAcessoEnum.Administrador or TipoAcessoEnum.Funcionario))
        {
            return Forbid();
        }

        var viewModel = ObterViewModelParaCreate(new AgendamentoModel(), tipoAcesso.Value, DateTime.Today.AddDays(1));
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(AgendamentoCrudViewModel viewModel)
    {
        var tipoAcesso = usuarioContextoService.ObterTipoAcesso();
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
            var idBarbeiroLogado = usuarioContextoService.ObterIdBarbeiro();
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
            return View(ObterViewModelParaCreate(viewModel.Agendamento, tipoAcesso.Value, viewModel.DataSelecionada));
        }

        try
        {
            agendamentoService.Criar(viewModel.Agendamento);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(ObterViewModelParaCreate(viewModel.Agendamento, tipoAcesso.Value, viewModel.DataSelecionada));
        }
    }

    [HttpGet]
    public IActionResult ObterSlotsDisponiveis(int barbeiroId, DateTime data)
    {
        if (barbeiroId <= 0)
        {
            return Json(new List<object>());
        }

        var slots = slotHorarioService.ObterSlotsDisponiveis(barbeiroId, data.Date)
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

        var barbeiro = barbeiroService.ObterPorId(barbeiroId);
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

    public IActionResult Edit(int id)
    {
        var agendamento = agendamentoService.ObterPorId(id, 0, TipoAcessoEnum.Administrador);
        return agendamento is null ? NotFound() : View(ObterViewModel(agendamento, agendamentoService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, AgendamentoCrudViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(ObterViewModel(viewModel.Agendamento, agendamentoService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)));
        }

        agendamentoService.AtualizarDoFuncionario(id, viewModel.Agendamento.Barbeiro.Id, viewModel.Agendamento, TipoAcessoEnum.Funcionario);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AtualizarStatus(int idAgendamento, StatusAgendamentoEnum status)
    {
        var tipoAcesso = usuarioContextoService.ObterTipoAcesso();
        if (!tipoAcesso.HasValue)
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            if (tipoAcesso.Value == TipoAcessoEnum.Funcionario)
            {
                var idBarbeiro = usuarioContextoService.ObterIdBarbeiro();
                if (!idBarbeiro.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                agendamentoService.AtualizarStatus(idAgendamento, status, TipoAcessoEnum.Funcionario, idBarbeiro.Value);
            }
            else if (tipoAcesso.Value == TipoAcessoEnum.Administrador)
            {
                agendamentoService.AtualizarStatus(idAgendamento, status, TipoAcessoEnum.Administrador);
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
        var agendamento = agendamentoService.ObterPorId(id, 0, TipoAcessoEnum.Administrador);
        return agendamento is null ? NotFound() : View(ObterViewModel(agendamento, agendamentoService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id, int barbeiroId)
    {
        agendamentoService.Excluir(id, barbeiroId, TipoAcessoEnum.Funcionario);
        return RedirectToAction(nameof(Index));
    }

    private AgendamentoCrudViewModel ObterViewModel(AgendamentoModel agendamento, List<AgendamentoModel> agendamentos)
    {
        var barbeiros = barbeiroService.ObterTodos();
        var clientes = clienteService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador);
        var procedimentos = procedimentoService.ObterTodos();

        return new AgendamentoCrudViewModel
        {
            Agendamento = agendamento,
            Agendamentos = agendamentos,
            Barbeiros = barbeiros,
            Clientes = clientes,
            Procedimentos = procedimentos,
            PrecosPorBarbeiroProcedimento = barbeiros.ToDictionary(x => x.Id, MontarMapaPrecosPorProcedimento)
        };
    }

    private AgendamentoCrudViewModel ObterViewModelSomenteHistoricoCliente(List<AgendamentoModel> historico)
    {
        return new AgendamentoCrudViewModel
        {
            Agendamento = new AgendamentoModel(),
            Agendamentos = historico,
            Barbeiros = [],
            Clientes = [],
            Procedimentos = [],
            PrecosPorBarbeiroProcedimento = new Dictionary<int, Dictionary<int, decimal>>()
        };
    }

    private AgendamentoCrudViewModel ObterViewModelParaCreate(AgendamentoModel agendamento, TipoAcessoEnum tipoAcesso, DateTime dataSelecionada)
    {
        List<BarbeiroModel> barbeiros;
        List<ClienteModel> clientes;

        if (tipoAcesso == TipoAcessoEnum.Funcionario)
        {
            var idBarbeiroLogado = usuarioContextoService.ObterIdBarbeiro();
            if (!idBarbeiroLogado.HasValue)
            {
                throw new UnauthorizedAccessException("Funcionario sem id de barbeiro na sessao.");
            }

            var barbeiro = barbeiroService.ObterPorId(idBarbeiroLogado.Value)
                ?? throw new KeyNotFoundException($"Barbeiro {idBarbeiroLogado.Value} nao encontrado.");

            barbeiros = [barbeiro];
            clientes = clienteService.ObterPorBarbeiro(idBarbeiroLogado.Value, TipoAcessoEnum.Funcionario)
                .Where(x => x.Ativo)
                .ToList();

            agendamento.Barbeiro = barbeiro;
            // Valor inicial da tela para refletir a regra do fluxo de cadastro pelo funcionario.
            agendamento.StatusAgendamentoEnum = StatusAgendamentoEnum.Agendado;
        }
        else
        {
            barbeiros = barbeiroService.ObterTodos()
                .Where(x => x.TipoAcesso is TipoAcessoEnum.Administrador or TipoAcessoEnum.Funcionario)
                .ToList();

            clientes = clienteService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)
                .Where(x => x.Ativo)
                .ToList();
        }

        ViewBag.PodeEscolherBarbeiro = tipoAcesso == TipoAcessoEnum.Administrador;

        var dataBase = dataSelecionada.Date;
        var barbeiroSelecionadoId = agendamento.Barbeiro?.Id > 0
            ? agendamento.Barbeiro.Id
            : barbeiros.FirstOrDefault()?.Id ?? 0;

        var barbeiroSelecionado = barbeiros.FirstOrDefault(x => x.Id == barbeiroSelecionadoId);
        var procedimentos = barbeiroSelecionado is not null
            ? ObterProcedimentosDisponiveisDoBarbeiro(barbeiroSelecionado)
            : new List<ProcedimentoModel>();

        var slotsDisponiveis = barbeiroSelecionadoId > 0
            ? slotHorarioService.ObterSlotsDisponiveis(barbeiroSelecionadoId, dataBase)
            : [];

        return new AgendamentoCrudViewModel
        {
            Agendamento = agendamento,
            Agendamentos = [],
            Barbeiros = barbeiros,
            Clientes = clientes,
            Procedimentos = procedimentos,
            DataSelecionada = dataBase,
            SlotsDisponiveis = slotsDisponiveis,
            PrecosPorBarbeiroProcedimento = barbeiros.ToDictionary(x => x.Id, MontarMapaPrecosPorProcedimento)
        };
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

    private List<ProcedimentoModel> ObterProcedimentosDisponiveisDoBarbeiro(BarbeiroModel barbeiro)
    {
        var possuiRelacoes = barbeiro.RelacoesProcedimentos.Any();

        if (possuiRelacoes)
        {
            return barbeiro.RelacoesProcedimentos
                .Where(x => x.Ativo)
                .Select(x => procedimentoService.ObterPorId(x.ProcedimentoId))
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

    private bool UsuarioEhClienteComAgendamentoVinculado(int idAgendamento)
    {
        var tipoAcesso = usuarioContextoService.ObterTipoAcesso();
        if (tipoAcesso != TipoAcessoEnum.Cliente)
        {
            return true;
        }

        var idCliente = usuarioContextoService.ObterIdCliente();
        if (!idCliente.HasValue)
        {
            return false;
        }

        var cliente = clienteService.ObterPerfilCliente(idCliente.Value, TipoAcessoEnum.Cliente);
        var agendamentoPertenceAoCliente = agendamentoService
            .ObterPorCpfCliente(cliente.CPF, TipoAcessoEnum.Cliente)
            .Any(x => x.IdAgendamento == idAgendamento);

        return agendamentoPertenceAoCliente;
    }
}