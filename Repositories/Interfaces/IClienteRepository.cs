using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Repositories.Interfaces
{
    public interface IClienteRepository
    {
        List<ClienteModel> ObterTodos();
        List<ClienteModel> ObterPorBarbeiroId(int barbeiroId);
        ClienteModel? ObterPorId(int id);
        ClienteModel? ObterPorCpf(string cpf);
        ClienteModel Adicionar(ClienteModel cliente);
        ClienteModel Atualizar(ClienteModel cliente);
        ClienteModel Desativar(int id);
        ClienteModel Ativar(int id);
        void Excluir(int id);
    }
}