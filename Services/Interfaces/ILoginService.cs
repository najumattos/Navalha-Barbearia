using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Services.Interfaces
{
    public interface ILoginService
    {
        LoginModel? Autenticar(string email, string senha);
        LoginModel? ObterPorBarbeiroId(int idBarbeiro);
        LoginModel? ObterPorClienteId(int idCliente);
    }
}