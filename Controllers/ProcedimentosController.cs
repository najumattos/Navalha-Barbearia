using Microsoft.AspNetCore.Mvc;
using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Models.ViewModels;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Controllers
{
    public class ProcedimentosController : Controller
    {
        private readonly IProcedimentoService _procedimentoService;
        private readonly IBarbeiroService _barbeiroService;
        private readonly IUsuarioContextoService _usuarioContextoService;

        public ProcedimentosController(IProcedimentoService procedimentoService, IBarbeiroService barbeiroService, IUsuarioContextoService usuarioContextoService)
        {
            _procedimentoService = procedimentoService;
            _barbeiroService = barbeiroService;
            _usuarioContextoService = usuarioContextoService;
        }

        public IActionResult Index()
        {
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            if (tipoAcesso == TipoAcessoEnum.Cliente)
            {
                return Forbid();
            }

            var exibirColunasFuncionario = tipoAcesso == TipoAcessoEnum.Funcionario;
            var vinculosFuncionario = new Dictionary<int, VinculoProcedimentoFuncionarioViewModel>();

            if (exibirColunasFuncionario)
            {
                var idBarbeiro = _usuarioContextoService.ObterIdBarbeiro();
                if (idBarbeiro.HasValue)
                {
                    var barbeiro = _barbeiroService.ObterPorId(idBarbeiro.Value);
                    if (barbeiro is not null)
                    {
                        vinculosFuncionario = barbeiro.RelacoesProcedimentos
                            .GroupBy(x => x.ProcedimentoId)
                            .ToDictionary(
                                x => x.Key,
                                x =>
                                {
                                    var vinculoAtivo = x.FirstOrDefault(v => v.Ativo);
                                    var vinculoMaisRecente = x.OrderByDescending(v => v.AtualizadoEm).First();
                                    return new VinculoProcedimentoFuncionarioViewModel
                                    {
                                        PrecoPorBarbeiro = vinculoAtivo?.PrecoPorBarbeiro ?? vinculoMaisRecente.PrecoPorBarbeiro,
                                        Ativo = vinculoAtivo?.Ativo ?? vinculoMaisRecente.Ativo
                                    };
                                });
                    }
                }
            }

            return View(new ProcedimentoCrudViewModel
            {
                Procedimentos = _procedimentoService.ObterTodos(),
                PodeGerenciarCatalogo = tipoAcesso == TipoAcessoEnum.Administrador,
                ExibirColunasFuncionario = exibirColunasFuncionario,
                VinculosFuncionarioPorProcedimentoId = vinculosFuncionario
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AtualizarAtivoFuncionario(int procedimentoId, bool ativo)
        {
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            if (tipoAcesso != TipoAcessoEnum.Funcionario)
            {
                return Forbid();
            }

            var idBarbeiro = _usuarioContextoService.ObterIdBarbeiro();
            if (!idBarbeiro.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                if (ativo)
                {
                    _barbeiroService.AdicionarProcedimentoAoBarbeiro(idBarbeiro.Value, procedimentoId, TipoAcessoEnum.Funcionario);
                }
                else
                {
                    _barbeiroService.RemoverProcedimentoDoBarbeiro(idBarbeiro.Value, procedimentoId, TipoAcessoEnum.Funcionario);
                }
            }
            catch (KeyNotFoundException)
            {
                // Mantemos fluxo silencioso para nao interromper a experiencia da tela de listagem.
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AtualizarPrecoPorBarbeiroFuncionario(int procedimentoId, decimal precoPorBarbeiro)
        {
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            if (tipoAcesso != TipoAcessoEnum.Funcionario)
            {
                return Forbid();
            }

            var idBarbeiro = _usuarioContextoService.ObterIdBarbeiro();
            if (!idBarbeiro.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                _barbeiroService.AtualizarPrecoPorBarbeiro(idBarbeiro.Value, procedimentoId, precoPorBarbeiro, TipoAcessoEnum.Funcionario);
            }
            catch (KeyNotFoundException)
            {
                // Mantemos fluxo silencioso para nao interromper a experiencia da tela de listagem.
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            if (_usuarioContextoService.ObterTipoAcesso() == TipoAcessoEnum.Cliente)
            {
                return Forbid();
            }

            var procedimento = _procedimentoService.ObterPorId(id);
            if (procedimento is null)
            {
                return NotFound();
            }

            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            var ehAdministrador = tipoAcesso == TipoAcessoEnum.Administrador;

            // A listagem de preco por barbeiro e exclusiva para o administrador para manter o escopo de permissao.
            var precosPorBarbeiro = ehAdministrador
                ? _barbeiroService.ObterTodos()
                    .Select(barbeiro => new PrecoProcedimentoPorBarbeiroViewModel
                    {
                        BarbeiroId = barbeiro.Id,
                        NomeBarbeiro = barbeiro.NomeCompleto,
                        PrecoPorBarbeiro = barbeiro.Procedimentos
                            .FirstOrDefault(x => x.Id == id)
                            ?.PrecoPorBarbeiro ?? procedimento.PrecoBase
                    })
                    .OrderBy(x => x.NomeBarbeiro)
                    .ToList()
                : new List<PrecoProcedimentoPorBarbeiroViewModel>();

            var viewModel = new ProcedimentoDetalhesViewModel
            {
                Id = procedimento.Id,
                Nome = procedimento.Nome,
                Descricao = procedimento.Descricao,
                PrecoBase = procedimento.PrecoBase,
                PodeVisualizarPrecosPorBarbeiro = ehAdministrador,
                PrecosPorBarbeiro = precosPorBarbeiro
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            var validacaoAdmin = ValidarAcessoAdministrador();
            if (validacaoAdmin is not null)
            {
                return validacaoAdmin;
            }

            return View(new ProcedimentoModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProcedimentoModel procedimento)
        {
            var validacaoAdmin = ValidarAcessoAdministrador();
            if (validacaoAdmin is not null)
            {
                return validacaoAdmin;
            }

            if (!ModelState.IsValid)
            {
                return View(procedimento);
            }

            _procedimentoService.Criar(procedimento, TipoAcessoEnum.Administrador);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var validacaoAdmin = ValidarAcessoAdministrador();
            if (validacaoAdmin is not null)
            {
                return validacaoAdmin;
            }

            var procedimento = _procedimentoService.ObterPorId(id);
            return procedimento is null ? NotFound() : View(procedimento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProcedimentoModel procedimento)
        {
            var validacaoAdmin = ValidarAcessoAdministrador();
            if (validacaoAdmin is not null)
            {
                return validacaoAdmin;
            }

            if (!ModelState.IsValid)
            {
                return View(procedimento);
            }

            _procedimentoService.AtualizarCatalogo(procedimento.Id, procedimento, TipoAcessoEnum.Administrador);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var validacaoAdmin = ValidarAcessoAdministrador();
            if (validacaoAdmin is not null)
            {
                return validacaoAdmin;
            }

            var procedimento = _procedimentoService.ObterPorId(id);
            return procedimento is null ? NotFound() : View(procedimento);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var validacaoAdmin = ValidarAcessoAdministrador();
            if (validacaoAdmin is not null)
            {
                return validacaoAdmin;
            }

            _procedimentoService.Excluir(id, TipoAcessoEnum.Administrador);
            return RedirectToAction(nameof(Index));
        }

        private IActionResult? ValidarAcessoAdministrador()
        {
            // Guard clause: falha cedo quando o usuario nao possui permissao para manter o fluxo simples e legivel.
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            if (!tipoAcesso.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (tipoAcesso.Value != TipoAcessoEnum.Administrador)
            {
                return Forbid();
            }

            return null;
        }
    }
}