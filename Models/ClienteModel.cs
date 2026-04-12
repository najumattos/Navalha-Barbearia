using Navalha_Barbearia.Enums;

namespace Navalha_Barbearia.Models
{
    public class ClienteModel
    {
        public int Id { get; set; }

        // Nome completo do cliente para exibicao e preenchimento automatico na Home.
        public string NomeCompleto { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        public string CPF { get; set; } = string.Empty;

        public string Endereco { get; set; } = string.Empty;

        public DateTime DataNascimento { get; set; }

        public GeneroEnum GeneroEnum { get; set; }

        // Valor padrao fixo para preservar o papel do cliente no dominio.
        public TipoAcessoEnum TipoAcesso { get; set; } = TipoAcessoEnum.Cliente;

        // Relacao de cadastro: o cliente sempre pertence ao barbeiro que o registrou.
        public BarbeiroModel BarbeiroQueCadastrou { get; set; } = new();

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public bool Ativo { get; set; } = true;
    }
}