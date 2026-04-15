using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Services.Interfaces
{
    public interface IAgendamentoService
    {
        List<AgendamentoModel> ObterTodosParaAdministrador(TipoAcessoEnum tipoAcessoSolicitante);
        List<AgendamentoModel> ObterPorBarbeiroId(int barbeiroId, TipoAcessoEnum tipoAcessoSolicitante);
        List<AgendamentoModel> ObterPorCpfCliente(string cpf, TipoAcessoEnum tipoAcessoSolicitante);
        List<AgendamentoModel> ObterHistoricoPorCpfParaEquipe(string cpf, TipoAcessoEnum tipoAcessoSolicitante);
        AgendamentoModel? ObterPorId(int idAgendamento, int barbeiroIdSolicitante, TipoAcessoEnum tipoAcessoSolicitante);
        AgendamentoModel Criar(AgendamentoModel agendamento);
        AgendamentoModel AtualizarDoFuncionario(int idAgendamento, int barbeiroIdSolicitante, AgendamentoModel agendamento, TipoAcessoEnum tipoAcessoSolicitante);
        AgendamentoModel AtualizarStatus(int idAgendamento, StatusAgendamentoEnum status, TipoAcessoEnum tipoAcessoSolicitante, int? barbeiroIdSolicitante = null, string? cpfClienteSolicitante = null);
        AgendamentoModel AtualizarStatusDoCliente(int idAgendamento, string cpfClienteSolicitante, StatusAgendamentoEnum status, TipoAcessoEnum tipoAcessoSolicitante);
        void Excluir(int idAgendamento, int barbeiroIdSolicitante, TipoAcessoEnum tipoAcessoSolicitante);
    }
}