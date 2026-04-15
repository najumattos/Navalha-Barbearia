using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Services.Interfaces
{
    public interface IClienteService
    {
        List<ClienteModel> ObterTodosParaAdministrador(TipoAcessoEnum tipoAcessoSolicitante);
        List<ClienteModel> ObterPorBarbeiro(int barbeiroId, TipoAcessoEnum tipoAcessoSolicitante);
        ClienteModel? ObterPorCpfPublico(string cpf);
        ClienteModel ObterPerfilCliente(int idCliente, TipoAcessoEnum tipoAcessoSolicitante);
        ClienteModel CadastrarPorBarbeiro(int barbeiroId, ClienteModel cliente, TipoAcessoEnum tipoAcessoSolicitante);
        ClienteModel AtualizarPorBarbeiro(int clienteId, int barbeiroId, ClienteModel cliente, TipoAcessoEnum tipoAcessoSolicitante);
        ClienteModel DesativarPorBarbeiro(int clienteId, int barbeiroId, TipoAcessoEnum tipoAcessoSolicitante);
        ClienteModel AtualizarPorAdministrador(int clienteId, ClienteModel cliente, TipoAcessoEnum tipoAcessoSolicitante);
        ClienteModel DesativarPorAdministrador(int clienteId, TipoAcessoEnum tipoAcessoSolicitante);
        ClienteModel AtivarPorAdministrador(int clienteId, TipoAcessoEnum tipoAcessoSolicitante);
    }
}