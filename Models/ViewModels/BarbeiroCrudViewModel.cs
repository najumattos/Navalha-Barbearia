namespace Navalha_Barbearia.Models.ViewModels
{
    public class BarbeiroCrudViewModel
    {
        public BarbeiroModel Barbeiro { get; set; } = new();

        public List<BarbeiroModel> Barbeiros { get; set; } = new();
    }
}