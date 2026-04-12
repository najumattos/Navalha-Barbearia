namespace Navalha_Barbearia.Models.ViewModels
{
    public class ClienteCrudViewModel
    {
        public ClienteModel Cliente { get; set; } = new();

        public List<ClienteModel> Clientes { get; set; } = new();

        public List<BarbeiroModel> Barbeiros { get; set; } = new();
    }
}