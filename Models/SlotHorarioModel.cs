using Navalha_Barbearia.Enums;

namespace Navalha_Barbearia.Models
{
    public class SlotHorarioModel
    {
        public int Id { get; set; }

        public DateTime Inicio { get; set; }

        public DateTime Fim { get; set; }

        public StatusHorarioEnum StatusHorarioEnum { get; set; } = StatusHorarioEnum.Livre;

        public int BarbeiroId { get; set; }
    }
}