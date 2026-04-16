using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;

namespace Navalha_Barbearia.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        // Repositorio em memoria para simplificar o fluxo de autenticacao durante o desenvolvimento.
        // Credenciais de exemplo para os perfis autenticados do sistema.
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
            },
            new LoginModel
            {
                Id = 3,
                IdBarbeiro = null,
                IdCliente = 1,
                Email = "123.456.789-00",
                Senha = "123456",
                TipoAcessoEnum = TipoAcessoEnum.Cliente
            }
        ];

        public LoginModel? ObterPorIdentificadorSenha(string identificador, string senha)
        {
            var identificadorNormalizado = NormalizarIdentificador(identificador);

            return _logins.FirstOrDefault(x => x.Senha == senha && (
                x.Email.Equals(identificador, StringComparison.OrdinalIgnoreCase) ||
                NormalizarIdentificador(x.Email) == identificadorNormalizado));
        }

        public LoginModel? ObterPorBarbeiroId(int idBarbeiro)
        {
            return _logins.FirstOrDefault(x => x.IdBarbeiro == idBarbeiro);
        }

        public LoginModel? ObterPorClienteId(int idCliente)
        {
            return _logins.FirstOrDefault(x => x.IdCliente == idCliente);
        }

        private static string NormalizarIdentificador(string valor)
        {
            return new string(valor.Where(char.IsDigit).ToArray());
        }

    }
}