using Navalha_Barbearia.Enums;

namespace Navalha_Barbearia.Models
{
    public class ProcedimentoModel
    {        
        private decimal _precoBase;
        private decimal? _precoPorBarbeiro;
        private bool _precoPorBarbeiroPersonalizado;

        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;

        // A descricao representa o cadastro padrao do procedimento.
        // Ela e mantida centralizada para evitar duplicidade e divergencia de regra.
        public string Descricao { get; set; } = string.Empty;

        // PrecoBase e a referencia padrao usada por todos os barbeiros.
        // O setter nao altera um preco personalizado ja configurado para o barbeiro.
        public decimal PrecoBase
        {
            get => _precoBase;
            set => _precoBase = value;
        }

        // PrecoPorBarbeiro usa o PrecoBase quando ainda nao existe uma cobranca customizada.
        // Isso reduz repeticao e deixa a regra de negocio explicita.
        public decimal PrecoPorBarbeiro
        {
            get => _precoPorBarbeiroPersonalizado && _precoPorBarbeiro.HasValue ? _precoPorBarbeiro.Value : PrecoBase;
            set
            {
                _precoPorBarbeiro = value;
                _precoPorBarbeiroPersonalizado = true;
            }
        }
    }
}