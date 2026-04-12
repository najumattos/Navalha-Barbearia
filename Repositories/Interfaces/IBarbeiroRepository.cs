using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Repositories.Interfaces
{
    public interface IBarbeiroRepository
    {
        List<BarbeiroModel> ObterTodos();
        BarbeiroModel? ObterPorId(int id);
        BarbeiroModel Adicionar(BarbeiroModel barbeiro);
        BarbeiroModel Atualizar(BarbeiroModel barbeiro);
        void Excluir(int id);
    }
}