namespace Navalha_Barbearia.Models.ViewModels
{
    public class HomeAdministradorViewModel
    {
        // O administrador enxerga a visao consolidada da barbearia.
        // Mantemos a tela simples com os dados prontos, sem regra de negocio na view.
        public List<Navalha_Barbearia.Models.BarbeiroModel> Barbeiros { get; set; } = new();

        public List<Navalha_Barbearia.Models.AgendamentoModel> Agendamentos { get; set; } = new();

        public List<Navalha_Barbearia.Models.ClienteModel> Clientes { get; set; } = new();
    }
}