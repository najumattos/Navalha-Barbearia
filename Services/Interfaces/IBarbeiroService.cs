using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Services.Interfaces
{
    public interface IBarbeiroService
    {
        List<BarbeiroModel> ObterTodos();
        BarbeiroModel? ObterPorId(int id);
        BarbeiroModel Criar(BarbeiroModel barbeiro);
        BarbeiroModel Atualizar(BarbeiroModel barbeiro);
        void Excluir(int id);

        ProcedimentoModel AdicionarProcedimentoAoBarbeiro(int barbeiroId, ProcedimentoEnum procedimentoEnum, TipoAcessoEnum tipoAcessoSolicitante);
        ProcedimentoModel RemoverProcedimentoDoBarbeiro(int barbeiroId, ProcedimentoEnum procedimentoEnum, TipoAcessoEnum tipoAcessoSolicitante);
        ProcedimentoModel AtualizarPrecoPorBarbeiro(int barbeiroId, ProcedimentoEnum procedimentoEnum, decimal precoPorBarbeiro, TipoAcessoEnum tipoAcessoSolicitante);
    }
}