namespace Navalha_Barbearia.Models.ViewModels
{
    public class ProcedimentoCrudViewModel
    {
        public ProcedimentoModel Procedimento { get; set; } = new();

        public List<ProcedimentoModel> Procedimentos { get; set; } = new();
    }
}