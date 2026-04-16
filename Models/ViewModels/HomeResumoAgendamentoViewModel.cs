using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Models.ViewModels
{
    public class HomeResumoAgendamentoViewModel
    {
        // Dados consolidados da etapa de confirmacao do agendamento publico.
        public ClienteModel Cliente { get; set; } = new();

        public AgendamentoModel AgendamentoAtual { get; set; } = new();

        public List<AgendamentoModel> HistoricoRecente { get; set; } = new();

        public bool ExibirBotaoConfirmar { get; set; }

        // Indica de onde o usuario veio: "agendamentos" para admin/funcionario, vazio para publico.
        public string? Origem { get; set; }
    }
}