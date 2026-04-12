using Microsoft.AspNetCore.Mvc;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Controllers
{
    public class BarbeirosController : Controller
    {
        private readonly IBarbeiroService _barbeiroService;

        public BarbeirosController(IBarbeiroService barbeiroService)
        {
            _barbeiroService = barbeiroService;
        }

        public IActionResult Index()
        {
            // A index exibe o inventario completo do dominio desta entidade.
            return View(_barbeiroService.ObterTodos());
        }

        public IActionResult Details(int id)
        {
            var barbeiro = _barbeiroService.ObterPorId(id);
            return barbeiro is null ? NotFound() : View(barbeiro);
        }

        public IActionResult Create()
        {
            return View(new BarbeiroModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BarbeiroModel barbeiro)
        {
            if (!ModelState.IsValid)
            {
                return View(barbeiro);
            }

            _barbeiroService.Criar(barbeiro);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var barbeiro = _barbeiroService.ObterPorId(id);
            return barbeiro is null ? NotFound() : View(barbeiro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BarbeiroModel barbeiro)
        {
            if (!ModelState.IsValid)
            {
                return View(barbeiro);
            }

            _barbeiroService.Atualizar(barbeiro);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var barbeiro = _barbeiroService.ObterPorId(id);
            return barbeiro is null ? NotFound() : View(barbeiro);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _barbeiroService.Excluir(id);
            return RedirectToAction(nameof(Index));
        }
    }
}