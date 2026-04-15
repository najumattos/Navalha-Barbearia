namespace Navalha_Barbearia.Models
{
    public class BarbeiroProcedimentoModel
    {
        public int BarbeiroId { get; set; }

        public int ProcedimentoId { get; set; }

        public decimal PrecoPorBarbeiro { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime AtualizadoEm { get; set; } = DateTime.Now;
    }
}