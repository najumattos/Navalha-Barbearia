using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Repositories.Interfaces
{
    public interface ILoginRepository
    {
        LoginModel? ObterPorIdentificadorSenha(string identificador, string senha);
        LoginModel? ObterPorBarbeiroId(int idBarbeiro);
        LoginModel? ObterPorClienteId(int idCliente);
    }
}