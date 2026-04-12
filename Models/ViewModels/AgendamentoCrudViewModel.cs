namespace Navalha_Barbearia.Models.ViewModels
{
    public class AgendamentoCrudViewModel
    {
        public AgendamentoModel Agendamento { get; set; } = new();

        public List<AgendamentoModel> Agendamentos { get; set; } = new();

        public List<BarbeiroModel> Barbeiros { get; set; } = new();

        public List<ClienteModel> Clientes { get; set; } = new();

        public List<ProcedimentoModel> Procedimentos { get; set; } = new();

        public Dictionary<int, Dictionary<int, decimal>> PrecosPorBarbeiroProcedimento { get; set; } = new();
    }
}