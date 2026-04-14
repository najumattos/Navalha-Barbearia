using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;

namespace Navalha_Barbearia.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        // Repositorio em memoria para simplificar o fluxo de autenticacao durante o desenvolvimento.
        // Apenas Funcionario e Administrador tem acesso ao login; Clientes nao podem acessar o sistema.
        private static readonly List<LoginModel> _logins =
        [
            new LoginModel
            {
                Id = 1,
                IdBarbeiro = 1,
                IdCliente = null,
                Email = "admin@navalha.com",
                Senha = "123456",
                TipoAcessoEnum = TipoAcessoEnum.Administrador
            },
            new LoginModel
            {
                Id = 2,
                IdBarbeiro = 2,
                IdCliente = null,
                Email = "funcionario@navalha.com",
                Senha = "123456",
                TipoAcessoEnum = TipoAcessoEnum.Funcionario
            }
        ];

        public LoginModel? ObterPorEmailSenha(string email, string senha)
        {
            return _logins.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && x.Senha == senha);
        }

        public LoginModel? ObterPorBarbeiroId(int idBarbeiro)
        {
            return _logins.FirstOrDefault(x => x.IdBarbeiro == idBarbeiro);
        }

        public LoginModel? ObterPorClienteId(int idCliente)
        {
            return _logins.FirstOrDefault(x => x.IdCliente == idCliente);
        }

    }
}