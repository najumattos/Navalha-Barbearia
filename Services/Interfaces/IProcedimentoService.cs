using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Services.Interfaces
{
    public interface IProcedimentoService
    {
        List<ProcedimentoModel> ObterTodos();
        ProcedimentoModel? ObterPorId(int id);
        ProcedimentoModel Criar(ProcedimentoModel procedimento, TipoAcessoEnum tipoAcessoSolicitante);
        ProcedimentoModel AtualizarCatalogo(int id, ProcedimentoModel procedimentoAtualizado, TipoAcessoEnum tipoAcessoSolicitante);
        void Excluir(int id, TipoAcessoEnum tipoAcessoSolicitante);
    }
}