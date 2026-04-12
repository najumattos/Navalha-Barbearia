using Microsoft.AspNetCore.Mvc;
using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models.ViewModels;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILoginService _loginService;

        public AuthController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginRequestViewModel loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(loginRequest);
            }

            var login = _loginService.Autenticar(loginRequest.Email, loginRequest.Senha);
            if (login is null)
            {
                // Feedback claro para evitar mensagens ambiguas e facilitar manutencao.
                ModelState.AddModelError(string.Empty, "E-mail ou senha invalidos.");
                return View(loginRequest);
            }

            if (login.TipoAcessoEnum == TipoAcessoEnum.Funcionario)
            {
                return RedirectToAction("HomeFuncionario", "Home", new { idBarbeiro = login.IdBarbeiro ?? 0 });
            }

            if (login.TipoAcessoEnum == TipoAcessoEnum.Administrador)
            {
                return RedirectToAction("HomeAdministrador", "Home", new { idBarbeiro = login.IdBarbeiro ?? 0 });
            }

            if (login.TipoAcessoEnum == TipoAcessoEnum.Cliente)
            {
                return RedirectToAction("HomeCliente", "Home", new { idCliente = login.IdCliente ?? 0 });
            }

            ModelState.AddModelError(string.Empty, "Perfil sem area de navegacao configurada.");
            return View(loginRequest);
        }
    }
}