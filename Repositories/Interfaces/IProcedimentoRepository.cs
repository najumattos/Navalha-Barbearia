using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Repositories.Interfaces
{
    public interface IProcedimentoRepository
    {
        List<ProcedimentoModel> ObterTodos();
        ProcedimentoModel? ObterPorId(int id);
        ProcedimentoModel Adicionar(ProcedimentoModel procedimento);
        ProcedimentoModel Atualizar(ProcedimentoModel procedimento);
        void Excluir(int id);
    }
}