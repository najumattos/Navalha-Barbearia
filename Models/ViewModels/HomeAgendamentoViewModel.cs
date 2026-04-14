using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Models.ViewModels
{
    public class HomeAgendamentoViewModel
    {
        // ViewModel: transporta apenas o que a tela precisa, sem expor toda a regra do dominio.
        public AgendamentoModel Agendamento { get; set; } = new();

        public List<BarbeiroModel> Barbeiros { get; set; } = new();

        public List<ProcedimentoModel> Procedimentos { get; set; } = new();

        // Mapa auxiliar para a tela: [IdBarbeiro] -> [ProcedimentoId] -> PrecoPorBarbeiro.
        // Boa pratica: manter esse dado no ViewModel evita consulta extra e deixa a UI previsivel.
        public Dictionary<int, Dictionary<int, decimal>> PrecosPorBarbeiroProcedimento { get; set; } = new();

        // Controle de UX: indica se o CPF informado encontrou um cliente ativo.
        public bool ClienteEncontradoPorCpf { get; set; }

        public DateTime DataSelecionada { get; set; } = DateTime.Today.AddDays(1);

        public List<SlotHorarioModel> SlotsDisponiveis { get; set; } = new();
    }
}