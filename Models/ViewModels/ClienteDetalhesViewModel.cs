namespace Navalha_Barbearia.Models.ViewModels
{
    public class ClienteDetalhesViewModel
    {
        public ClienteModel Cliente { get; set; } = new();

        public List<AgendamentoModel> HistoricoAgendamentos { get; set; } = new();

        public bool PodeDesativar { get; set; }
    }
}