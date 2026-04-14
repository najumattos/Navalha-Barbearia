using Navalha_Barbearia.Enums;

namespace Navalha_Barbearia.Models.ViewModels
{
    public class ProcedimentoDetalhesViewModel
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public decimal PrecoBase { get; set; }

        public bool PodeVisualizarPrecosPorBarbeiro { get; set; }

        public List<PrecoProcedimentoPorBarbeiroViewModel> PrecosPorBarbeiro { get; set; } = new();
    }

    public class PrecoProcedimentoPorBarbeiroViewModel
    {
        public int BarbeiroId { get; set; }

        public string NomeBarbeiro { get; set; } = string.Empty;

        public decimal PrecoPorBarbeiro { get; set; }
    }
}