using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Services.Interfaces
{
    public interface IAgendamentoValidacaoService
    {
        SlotHorarioModel ValidarCriacaoComSlotLivre(AgendamentoModel agendamento);
    }
}