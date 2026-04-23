using Microsoft.AspNetCore.Mvc;
using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models.ViewModels;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Controllers;

public class AuthController(ILoginService loginService, IUsuarioContextoService usuarioContextoService) : Controller
{
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

        var login = loginService.Autenticar(loginRequest.Identificador, loginRequest.Senha);
        if (login is null)
        {
            // Feedback claro evita ambiguidade para usuaria e para manutencao futura.
            ModelState.AddModelError(string.Empty, "E-mail/CPF ou senha invalidos.");
            return View(loginRequest);
        }

        // SRP: a responsabilidade de armazenar contexto de login fica no service dedicado.
        usuarioContextoService.DefinirContextoLogin(login.TipoAcessoEnum, login.IdBarbeiro, login.IdCliente);

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
            return RedirectToAction("Index", "Agendamentos");
        }

        ModelState.AddModelError(string.Empty, "Perfil nao tem permissao de acesso.");
        return View(loginRequest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        usuarioContextoService.LimparContextoLogin();
        return RedirectToAction("Index", "Home");
    }
}