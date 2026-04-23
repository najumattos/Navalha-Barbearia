using Microsoft.AspNetCore.Mvc;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Models.ViewModels;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Controllers;

public class BarbeirosController(IBarbeiroService barbeiroService, IProcedimentoService procedimentoService) : Controller
{
    public IActionResult Index()
    {
        // A index exibe o inventario completo do dominio desta entidade.
        return View(barbeiroService.ObterTodos());
    }

    public IActionResult Details(int id)
    {
        var barbeiro = barbeiroService.ObterPorId(id);
        if (barbeiro is null)
        {
            return NotFound();
        }

        ViewBag.ProcedimentosDoBarbeiro = MontarProcedimentosDoBarbeiro(barbeiro);
        return View(barbeiro);
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

        barbeiroService.Criar(barbeiro);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var barbeiro = barbeiroService.ObterPorId(id);
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

        barbeiroService.Atualizar(barbeiro);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var barbeiro = barbeiroService.ObterPorId(id);
        return barbeiro is null ? NotFound() : View(barbeiro);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        barbeiroService.Excluir(id);
        return RedirectToAction(nameof(Index));
    }

    private List<ProcedimentoDoBarbeiroViewModel> MontarProcedimentosDoBarbeiro(BarbeiroModel barbeiro)
    {
        var catalogo = procedimentoService.ObterTodos().ToDictionary(x => x.Id, x => x);
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
                PrecoPorBarbeiro = relacao.PrecoPorBarbeiro
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
                PrecoPorBarbeiro = procedimentoLegado.PrecoPorBarbeiro
            });
        }

        return procedimentos.OrderBy(x => x.Nome).ToList();
    }
}