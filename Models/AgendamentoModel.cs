using Navalha_Barbearia.Enums;

namespace Navalha_Barbearia.Models
{
    public class AgendamentoModel
    {
        // Id de negocio do agendamento. Mantemos o nome explicito para leitura facil.
        public int IdAgendamento { get; set; }

        // Relacao com o cliente: substitui dados soltos (nome, telefone, cpf) para evitar duplicidade.
        public ClienteModel Cliente { get; set; } = new();

        // Relacao de entidade: o agendamento aponta para um barbeiro do dominio.
        // Isso evita texto solto e melhora a consistencia dos dados entre telas e regras de negocio.
        public BarbeiroModel Barbeiro { get; set; } = new();

        // O procedimento eh ligado ao dominio existente via enum, evitando strings livres.
        public ProcedimentoEnum Procedimento { get; set; }

        // Referencia ao slot escolhido no calendario de horarios do barbeiro.
        public int SlotHorarioId { get; set; }

        public DateTime DataHora { get; set; }

        // Pendente e a situacao padrao para um novo agendamento.
        public StatusAgendamentoEnum StatusAgendamentoEnum { get; set; } = StatusAgendamentoEnum.Pendente;

        public decimal Preco { get; set; }

        public string Observacao { get; set; } = string.Empty;
    }
}