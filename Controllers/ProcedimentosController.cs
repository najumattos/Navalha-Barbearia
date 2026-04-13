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
            if (tipoAcesso == TipoAcessoEnum.Funcionario)
            {
                var idBarbeiro = _usuarioContextoService.ObterIdBarbeiro();
                if (!idBarbeiro.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var barbeiro = _barbeiroService.ObterPorId(idBarbeiro.Value);
                if (barbeiro is null)
                {
                    return NotFound();
                }

                return View(new ProcedimentoCrudViewModel
                {
                    Procedimentos = barbeiro.Procedimentos
                });
            }

            return View(new ProcedimentoCrudViewModel
            {
                Procedimentos = _procedimentoService.ObterTodos()
            });
        }

        public IActionResult Details(ProcedimentoEnum procedimentoEnum)
        {
            var procedimento = _procedimentoService.ObterPorTipo(procedimentoEnum);
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
                            .FirstOrDefault(x => x.ProcedimentoEnum == procedimentoEnum)
                            ?.PrecoPorBarbeiro ?? procedimento.PrecoBase
                    })
                    .OrderBy(x => x.NomeBarbeiro)
                    .ToList()
                : new List<PrecoProcedimentoPorBarbeiroViewModel>();

            var viewModel = new ProcedimentoDetalhesViewModel
            {
                ProcedimentoEnum = procedimento.ProcedimentoEnum,
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

        public IActionResult Edit(ProcedimentoEnum? procedimentoEnum)
        {
            var validacaoAdmin = ValidarAcessoAdministrador();
            if (validacaoAdmin is not null)
            {
                return validacaoAdmin;
            }

            // Mantemos um unico ponto de preparacao da tela para seguir DRY e facilitar manutencao.
            var procedimentoSelecionado = procedimentoEnum ?? _procedimentoService.ObterTodos().Select(x => x.ProcedimentoEnum).FirstOrDefault();
            var procedimento = _procedimentoService.ObterPorTipo(procedimentoSelecionado);

            ViewBag.TiposProcedimento = Enum.GetValues<ProcedimentoEnum>();
            return procedimento is null ? NotFound() : View(procedimento);
        }

        [HttpGet]
        public IActionResult BuscarPorTipo(ProcedimentoEnum procedimentoEnum)
        {
            var validacaoAdmin = ValidarAcessoAdministrador();
            if (validacaoAdmin is not null)
            {
                return validacaoAdmin;
            }

            var procedimento = _procedimentoService.ObterPorTipo(procedimentoEnum);
            if (procedimento is null)
            {
                return Json(new { encontrado = false });
            }

            // Endpoint de leitura simples para auto preenchimento na UI, sem duplicar regra de negocio.
            return Json(new
            {
                encontrado = true,
                procedimentoEnum = (int)procedimento.ProcedimentoEnum,
                descricao = procedimento.Descricao,
                precoBase = procedimento.PrecoBase
            });
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
                ViewBag.TiposProcedimento = Enum.GetValues<ProcedimentoEnum>();
                return View(procedimento);
            }

            _procedimentoService.AtualizarCatalogo(procedimento.ProcedimentoEnum, procedimento, TipoAcessoEnum.Administrador);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(ProcedimentoEnum procedimentoEnum)
        {
            var validacaoAdmin = ValidarAcessoAdministrador();
            if (validacaoAdmin is not null)
            {
                return validacaoAdmin;
            }

            var procedimento = _procedimentoService.ObterPorTipo(procedimentoEnum);
            return procedimento is null ? NotFound() : View(procedimento);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(ProcedimentoEnum procedimentoEnum)
        {
            var validacaoAdmin = ValidarAcessoAdministrador();
            if (validacaoAdmin is not null)
            {
                return validacaoAdmin;
            }

            _procedimentoService.Excluir(procedimentoEnum, TipoAcessoEnum.Administrador);
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