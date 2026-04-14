using Navalha_Barbearia.Enums;

namespace Navalha_Barbearia.Models
{
    public class BarbeiroModel
    {
        public int Id { get; set; }

        public string NomeCompleto { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        // Relacao de agregacao: o barbeiro possui sua propria lista de procedimentos.
        // Essa estrutura ajuda a separar o cadastro do procedimento da cobranca praticada por barbeiro.
        public List<ProcedimentoModel> Procedimentos { get; set; } = new();

        // Estrutura de transicao para evoluir ao relacionamento N:N sem quebrar telas legadas.
        public List<BarbeiroProcedimentoModel> RelacoesProcedimentos { get; set; } = new();

        // Cada barbeiro possui sua carteira de clientes cadastrados por ele.
        // Essa relacao permite aplicar autorizacao por dono na camada de service.
        public List<ClienteModel> Clientes { get; set; } = new();

        public TipoAcessoEnum TipoAcesso { get; set; }
    }
}
