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
        private readonly IAgendamentoService _agendamentoService;
        private readonly IBarbeiroService _barbeiroService;
        private readonly IUsuarioContextoService _usuarioContextoService;

        public ClientesController(
            IClienteService clienteService,
            IAgendamentoService agendamentoService,
            IBarbeiroService barbeiroService,
            IUsuarioContextoService usuarioContextoService)
        {
            _clienteService = clienteService;
            _agendamentoService = agendamentoService;
            _barbeiroService = barbeiroService;
            _usuarioContextoService = usuarioContextoService;
        }

        public IActionResult Index()
        {
            // O mesmo endpoint Index atende perfis diferentes sem duplicar controller (DRY).
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            if (!tipoAcesso.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.PodeVerArquivados = tipoAcesso.Value == TipoAcessoEnum.Administrador;

            if (tipoAcesso.Value == TipoAcessoEnum.Funcionario)
            {
                var idBarbeiro = _usuarioContextoService.ObterIdBarbeiro();
                if (!idBarbeiro.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                return View(new ClienteCrudViewModel
                {
                    // A listagem principal mostra apenas clientes ativos; arquivados ficam em tela dedicada.
                    Clientes = _clienteService.ObterPorBarbeiro(idBarbeiro.Value, TipoAcessoEnum.Funcionario).Where(x => x.Ativo).ToList(),
                    Barbeiros = _barbeiroService.ObterTodos().Where(x => x.Id == idBarbeiro.Value).ToList()
                });
            }

            return View(new ClienteCrudViewModel
            {
                Clientes = _clienteService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador).Where(x => x.Ativo).ToList(),
                Barbeiros = _barbeiroService.ObterTodos()
            });
        }

        public IActionResult Arquivados()
        {
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            if (tipoAcesso != TipoAcessoEnum.Administrador)
            {
                return Forbid();
            }

            return View(new ClienteCrudViewModel
            {
                Clientes = _clienteService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador).Where(x => !x.Ativo).ToList(),
                Barbeiros = _barbeiroService.ObterTodos()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Ativar(int id)
        {
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            if (tipoAcesso != TipoAcessoEnum.Administrador)
            {
                return Forbid();
            }

            _clienteService.AtivarPorAdministrador(id, TipoAcessoEnum.Administrador);
            return RedirectToAction(nameof(Arquivados));
        }

        public IActionResult Details(int id)
        {
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            if (!tipoAcesso.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            ClienteModel? cliente;
            if (tipoAcesso.Value == TipoAcessoEnum.Administrador)
            {
                cliente = _clienteService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)
                    .FirstOrDefault(x => x.Id == id);
            }
            else if (tipoAcesso.Value == TipoAcessoEnum.Funcionario)
            {
                var idBarbeiro = _usuarioContextoService.ObterIdBarbeiro();
                if (!idBarbeiro.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                cliente = _clienteService.ObterPorBarbeiro(idBarbeiro.Value, TipoAcessoEnum.Funcionario)
                    .FirstOrDefault(x => x.Id == id);
            }
            else
            {
                return Forbid();
            }

            if (cliente is null)
            {
                return NotFound();
            }

            var viewModel = new ClienteDetalhesViewModel
            {
                Cliente = cliente,
                HistoricoAgendamentos = _agendamentoService.ObterHistoricoPorCpfParaEquipe(cliente.CPF, tipoAcesso.Value),
                PodeDesativar = tipoAcesso.Value == TipoAcessoEnum.Administrador
            };

            return View(viewModel);
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

            var barbeiros = _barbeiroService.ObterTodos();
            if (tipoAcesso.Value == TipoAcessoEnum.Funcionario)
            {
                var idBarbeiro = _usuarioContextoService.ObterIdBarbeiro();
                if (!idBarbeiro.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                barbeiros = barbeiros.Where(x => x.Id == idBarbeiro.Value).ToList();
            }

            ViewBag.PodeEscolherBarbeiro = tipoAcesso.Value == TipoAcessoEnum.Administrador;

            return View(new ClienteCrudViewModel
            {
                Cliente = new ClienteModel(),
                Barbeiros = barbeiros
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ClienteCrudViewModel viewModel)
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

            if (!ModelState.IsValid)
            {
                var barbeiros = _barbeiroService.ObterTodos();
                if (tipoAcesso.Value == TipoAcessoEnum.Funcionario)
                {
                    var idBarbeiroFuncionario = _usuarioContextoService.ObterIdBarbeiro();
                    if (!idBarbeiroFuncionario.HasValue)
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    barbeiros = barbeiros.Where(x => x.Id == idBarbeiroFuncionario.Value).ToList();
                }

                ViewBag.PodeEscolherBarbeiro = tipoAcesso.Value == TipoAcessoEnum.Administrador;
                viewModel.Barbeiros = barbeiros;
                return View(viewModel);
            }

            if (tipoAcesso.Value == TipoAcessoEnum.Administrador)
            {
                _clienteService.CadastrarPorBarbeiro(viewModel.Cliente.BarbeiroQueCadastrou.Id, viewModel.Cliente, TipoAcessoEnum.Administrador);
            }
            else
            {
                var idBarbeiroFuncionario = _usuarioContextoService.ObterIdBarbeiro();
                if (!idBarbeiroFuncionario.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                _clienteService.CadastrarPorBarbeiro(idBarbeiroFuncionario.Value, viewModel.Cliente, TipoAcessoEnum.Funcionario);
            }

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

        public IActionResult Desativar(int id)
        {
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            if (tipoAcesso != TipoAcessoEnum.Administrador)
            {
                return Forbid();
            }

            var cliente = _clienteService.ObterTodosParaAdministrador(TipoAcessoEnum.Administrador)
                .FirstOrDefault(x => x.Id == id && x.Ativo);

            return cliente is null ? NotFound() : View(cliente);
        }

        [HttpPost, ActionName("Desativar")]
        [ValidateAntiForgeryToken]
        public IActionResult DesativarConfirmed(int id)
        {
            var tipoAcesso = _usuarioContextoService.ObterTipoAcesso();
            if (tipoAcesso != TipoAcessoEnum.Administrador)
            {
                return Forbid();
            }

            _clienteService.DesativarPorAdministrador(id, TipoAcessoEnum.Administrador);
            return RedirectToAction(nameof(Index));
        }
    }
}