using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Repositories.Interfaces
{
    public interface ILoginRepository
    {
        LoginModel? ObterPorEmailSenha(string email, string senha);
        LoginModel? ObterPorBarbeiroId(int idBarbeiro);
        LoginModel? ObterPorClienteId(int idCliente);
    }
}