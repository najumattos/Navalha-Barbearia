using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Services.Interfaces
{
    public interface ISlotHorarioService
    {
        List<SlotHorarioModel> GerarSlotsDoDia(int barbeiroId, DateTime data);
        List<SlotHorarioModel> ObterSlotsDisponiveis(int barbeiroId, DateTime data);
        SlotHorarioModel? ObterPorId(int slotId);
        void OcuparSlot(int slotId);
    }
}