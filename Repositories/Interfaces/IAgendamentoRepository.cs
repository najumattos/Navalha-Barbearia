using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Repositories.Interfaces
{
    public interface IAgendamentoRepository
    {
        List<AgendamentoModel> ObterTodos();
        List<AgendamentoModel> ObterPorBarbeiroId(int barbeiroId);
        List<AgendamentoModel> ObterPorCpfCliente(string cpf);
        AgendamentoModel? ObterPorId(int idAgendamento);
        AgendamentoModel Adicionar(AgendamentoModel agendamento);
        AgendamentoModel Atualizar(AgendamentoModel agendamento);
        void Excluir(int idAgendamento);
    }
}