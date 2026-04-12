using Microsoft.AspNetCore.Mvc;
using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Models.ViewModels;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly IBarbeiroService _barbeiroService;
        private readonly IUsuarioContextoService _usuarioContextoService;

        public ClientesController(IClienteService clienteService, IBarbeiroService barbeiroService, IUsuarioContextoService usuarioContextoService)
        {
            _clienteService = clienteService;
            _barbeiroService = barbeiroService;
            _usuarioContextoService = usuarioContextoService;
        }

        public IActionResult Index()
        {
            // O mesmo endpoint Index atende perfis diferentes sem duplicar controller (DRY).
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();

            if (tipoAcesso == TipoAcessoEnum.Funcionario)
            {
                var idBarbeiro = _usuarioContextoService.ObterIdBarbeiro();
                if (!idBarbeiro.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                return View(new ClienteCrudViewModel
                {
                    Clientes = _clienteService.ObterPorBarbeiro(idBarbeiro.Value, TipoAcessoEnum.Funcionario),
                    Barbeiros = _barbeiroService.ObterTodos().Where(x => x.Id == idBarbeiro.Value).ToList()
                });
            }

            return View(new ClienteCrudViewModel
            {
                Clientes = _clienteService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador),
                Barbeiros = _barbeiroService.ObterTodos()
            });
        }

        public IActionResult Details(int id)
        {
            var cliente = _clienteService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)
                .FirstOrDefault(x => x.Id == id);

            return cliente is null ? NotFound() : View(cliente);
        }

        public IActionResult Create()
        {
            return View(new ClienteCrudViewModel
            {
                Cliente = new ClienteModel(),
                Barbeiros = _barbeiroService.ObterTodos()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ClienteCrudViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Barbeiros = _barbeiroService.ObterTodos();
                return View(viewModel);
            }

            _clienteService.CadastrarPorBarbeiro(viewModel.Cliente.BarbeiroQueCadastrou.Id, viewModel.Cliente, TipoAcessoEnum.Administrador);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var cliente = _clienteService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)
                .FirstOrDefault(x => x.Id == id);

            if (cliente is null)
            {
                return NotFound();
            }

            return View(new ClienteCrudViewModel
            {
                Cliente = cliente,
                Barbeiros = _barbeiroService.ObterTodos()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ClienteCrudViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Barbeiros = _barbeiroService.ObterTodos();
                return View(viewModel);
            }

            _clienteService.AtualizarPorAdministrador(id, viewModel.Cliente, TipoAcessoEnum.Administrador);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var cliente = _clienteService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)
                .FirstOrDefault(x => x.Id == id);

            return cliente is null ? NotFound() : View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _clienteService.ExcluirPorAdministrador(id, TipoAcessoEnum.Administrador);
            return RedirectToAction(nameof(Index));
        }
    }
}