namespace Navalha_Barbearia.Models.ViewModels
{
    public class ProcedimentoCrudViewModel
    {
        public ProcedimentoModel Procedimento { get; set; } = new();

        public List<ProcedimentoModel> Procedimentos { get; set; } = new();

        public bool PodeGerenciarCatalogo { get; set; }

        public bool ExibirColunasFuncionario { get; set; }

        public Dictionary<int, VinculoProcedimentoFuncionarioViewModel> VinculosFuncionarioPorProcedimentoId { get; set; } = new();
    }

    public class VinculoProcedimentoFuncionarioViewModel
    {
        public decimal? PrecoPorBarbeiro { get; set; }

        public bool? Ativo { get; set; }
    }
}