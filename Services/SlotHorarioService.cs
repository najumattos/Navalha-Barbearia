using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Services
{
    public class SlotHorarioService : ISlotHorarioService
    {
        private static readonly List<SlotHorarioModel> _slots = [];
        private static int _proximoId = 1;

        private readonly IAgendamentoRepository _agendamentoRepository;

        public SlotHorarioService(IAgendamentoRepository agendamentoRepository)
        {
            _agendamentoRepository = agendamentoRepository;
        }

        public List<SlotHorarioModel> GerarSlotsDoDia(int barbeiroId, DateTime data)
        {
            if (barbeiroId <= 0)
            {
                throw new ArgumentException("Barbeiro invalido para geracao de slots.");
            }

            var dia = data.Date;
            var slotsExistentes = _slots
                .Where(x => x.BarbeiroId == barbeiroId && x.Inicio.Date == dia)
                .OrderBy(x => x.Inicio)
                .ToList();

            if (slotsExistentes.Count == 0)
            {
                var inicio = dia.AddHours(8);
                var fim = dia.AddHours(18);

                for (var horario = inicio; horario < fim; horario = horario.AddMinutes(30))
                {
                    slotsExistentes.Add(new SlotHorarioModel
                    {
                        Id = _proximoId++,
                        Inicio = horario,
                        Fim = horario.AddMinutes(30),
                        StatusHorarioEnum = StatusHorarioEnum.Livre,
                        BarbeiroId = barbeiroId
                    });
                }

                _slots.AddRange(slotsExistentes);
            }

            SincronizarComAgendamentos(barbeiroId, dia, slotsExistentes);

            return slotsExistentes
                .OrderBy(x => x.Inicio)
                .ToList();
        }

        public List<SlotHorarioModel> ObterSlotsDisponiveis(int barbeiroId, DateTime data)
        {
            return GerarSlotsDoDia(barbeiroId, data)
                .Where(x => x.StatusHorarioEnum == StatusHorarioEnum.Livre)
                .ToList();
        }

        public SlotHorarioModel? ObterPorId(int slotId)
        {
            return _slots.FirstOrDefault(x => x.Id == slotId);
        }

        public void OcuparSlot(int slotId)
        {
            var slot = ObterPorId(slotId)
                ?? throw new KeyNotFoundException($"Slot de horario {slotId} nao encontrado.");

            if (slot.StatusHorarioEnum != StatusHorarioEnum.Livre)
            {
                throw new InvalidOperationException("Apenas slots livres podem ser ocupados.");
            }

            slot.StatusHorarioEnum = StatusHorarioEnum.Ocupado;
        }

        private void SincronizarComAgendamentos(int barbeiroId, DateTime dia, List<SlotHorarioModel> slotsDoDia)
        {
            var agendamentosDoDia = _agendamentoRepository.ObterPorBarbeiroId(barbeiroId)
                .Where(x => x.DataHora.Date == dia && x.StatusAgendamentoEnum != StatusAgendamentoEnum.Cancelado)
                .ToList();

            foreach (var slot in slotsDoDia)
            {
                if (slot.StatusHorarioEnum == StatusHorarioEnum.Bloqueado)
                {
                    continue;
                }

                var ocupado = agendamentosDoDia.Any(x => x.DataHora == slot.Inicio || x.SlotHorarioId == slot.Id);
                slot.StatusHorarioEnum = ocupado ? StatusHorarioEnum.Ocupado : StatusHorarioEnum.Livre;
            }
        }
    }
}