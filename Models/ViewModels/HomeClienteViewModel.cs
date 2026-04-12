using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Models.ViewModels
{
    public class HomeClienteViewModel
    {
        // ViewModel focado no perfil do cliente: dados cadastrais + seus agendamentos.
        public ClienteModel Cliente { get; set; } = new();

        public List<AgendamentoModel> Agendamentos { get; set; } = new();
    }
}