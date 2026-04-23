using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Services;

public class LoginService(ILoginRepository loginRepository) : ILoginService
{
    public LoginModel? Autenticar(string identificador, string senha)
    {
        // SRP: a validacao de credenciais fica centralizada no service de login.
        return loginRepository.ObterPorIdentificadorSenha(identificador, senha);
    }

    public LoginModel? ObterPorBarbeiroId(int idBarbeiro)
    {
        return loginRepository.ObterPorBarbeiroId(idBarbeiro);
    }

    public LoginModel? ObterPorClienteId(int idCliente)
    {
        return loginRepository.ObterPorClienteId(idCliente);
    }
}