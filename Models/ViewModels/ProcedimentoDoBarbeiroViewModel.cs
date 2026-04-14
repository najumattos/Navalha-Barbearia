namespace Navalha_Barbearia.Models.ViewModels
{
    public class ProcedimentoDoBarbeiroViewModel
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public decimal PrecoBase { get; set; }

        public decimal PrecoPorBarbeiro { get; set; }

        public bool Ativo { get; set; }
    }
}