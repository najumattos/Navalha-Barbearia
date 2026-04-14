using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILoginRepository _loginRepository;

        public LoginService(ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        public LoginModel? Autenticar(string email, string senha)
        {
            // SRP: a validacao de credenciais fica centralizada no service de login.
            var login = _loginRepository.ObterPorEmailSenha(email, senha);
            
            // Apenas Funcionario e Administrador tem acesso ao sistema.
            // Clientes sao automaticamente rejeitados mesmo que credenciais estejam corretas.
            if (login?.TipoAcessoEnum == TipoAcessoEnum.Cliente)
            {
                return null;
            }
            
            return login;
        }

        public LoginModel? ObterPorBarbeiroId(int idBarbeiro)
        {
            return _loginRepository.ObterPorBarbeiroId(idBarbeiro);
        }

        public LoginModel? ObterPorClienteId(int idCliente)
        {
            return _loginRepository.ObterPorClienteId(idCliente);
        }
    }
}