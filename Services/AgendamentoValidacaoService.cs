using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Services
{
    public class AgendamentoValidacaoService : IAgendamentoValidacaoService
    {
        private readonly ISlotHorarioService _slotHorarioService;
        private readonly IBarbeiroRepository _barbeiroRepository;
        private readonly IClienteRepository _clienteRepository;

        public AgendamentoValidacaoService(
            ISlotHorarioService slotHorarioService,
            IBarbeiroRepository barbeiroRepository,
            IClienteRepository clienteRepository)
        {
            _slotHorarioService = slotHorarioService;
            _barbeiroRepository = barbeiroRepository;
            _clienteRepository = clienteRepository;
        }

        public SlotHorarioModel ValidarCriacaoComSlotLivre(AgendamentoModel agendamento)
        {
            if (agendamento.Barbeiro?.Id <= 0)
            {
                throw new ArgumentException("Barbeiro invalido para o agendamento.");
            }

            if (agendamento.SlotHorarioId <= 0)
            {
                throw new ArgumentException("Selecione um slot de horario para continuar.");
            }

            if (string.IsNullOrWhiteSpace(agendamento.Cliente?.CPF))
            {
                throw new ArgumentException("CPF do cliente e obrigatorio para agendamento.");
            }

            var barbeiro = _barbeiroRepository.ObterPorId(agendamento.Barbeiro.Id)
                ?? throw new KeyNotFoundException($"Barbeiro {agendamento.Barbeiro.Id} nao encontrado.");

            _ = _clienteRepository.ObterPorCpf(agendamento.Cliente.CPF)
                ?? throw new KeyNotFoundException($"Cliente com CPF {agendamento.Cliente.CPF} nao encontrado.");

            var slot = _slotHorarioService.ObterPorId(agendamento.SlotHorarioId)
                ?? throw new KeyNotFoundException($"Slot de horario {agendamento.SlotHorarioId} nao encontrado.");

            if (slot.BarbeiroId != barbeiro.Id)
            {
                throw new ArgumentException("O slot selecionado nao pertence ao barbeiro informado.");
            }

            if (slot.StatusHorarioEnum != StatusHorarioEnum.Livre)
            {
                throw new InvalidOperationException("Este horario nao esta mais livre. Escolha outro slot.");
            }

            return slot;
        }
    }
}