using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Services.Interfaces
{
    public interface IProcedimentoService
    {
        List<ProcedimentoModel> ObterTodos();
        ProcedimentoModel? ObterPorTipo(ProcedimentoEnum procedimentoEnum);
        ProcedimentoModel Criar(ProcedimentoModel procedimento, TipoAcessoEnum tipoAcessoSolicitante);
        ProcedimentoModel AtualizarCatalogo(ProcedimentoEnum procedimentoEnum, ProcedimentoModel procedimentoAtualizado, TipoAcessoEnum tipoAcessoSolicitante);
        void Excluir(ProcedimentoEnum procedimentoEnum, TipoAcessoEnum tipoAcessoSolicitante);
    }
}