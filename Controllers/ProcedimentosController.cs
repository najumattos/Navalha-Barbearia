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
            return procedimento is null ? NotFound() : View(procedimento);
        }

        public IActionResult Create()
        {
            return View(new ProcedimentoModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProcedimentoModel procedimento)
        {
            if (!ModelState.IsValid)
            {
                return View(procedimento);
            }

            _procedimentoService.Criar(procedimento, TipoAcessoEnum.Administrador);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(ProcedimentoEnum procedimentoEnum)
        {
            var procedimento = _procedimentoService.ObterPorTipo(procedimentoEnum);
            return procedimento is null ? NotFound() : View(procedimento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProcedimentoEnum procedimentoEnum, ProcedimentoModel procedimento)
        {
            if (!ModelState.IsValid)
            {
                return View(procedimento);
            }

            _procedimentoService.AtualizarCatalogo(procedimentoEnum, procedimento, TipoAcessoEnum.Administrador);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(ProcedimentoEnum procedimentoEnum)
        {
            var procedimento = _procedimentoService.ObterPorTipo(procedimentoEnum);
            return procedimento is null ? NotFound() : View(procedimento);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(ProcedimentoEnum procedimentoEnum)
        {
            _procedimentoService.Excluir(procedimentoEnum, TipoAcessoEnum.Administrador);
            return RedirectToAction(nameof(Index));
        }
    }
}