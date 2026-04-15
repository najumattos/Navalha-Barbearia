using Navalha_Barbearia.Models;

namespace Navalha_Barbearia.Models.ViewModels
{
    public class HomeFuncionarioViewModel
    {
        // ViewModel simples e focado: evita acoplamento desnecessario com o model completo do barbeiro.
        public int IdBarbeiro { get; set; }

        public string NomeFuncionario { get; set; } = string.Empty;

        public List<ProcedimentoDoBarbeiroViewModel> ProcedimentosDoBarbeiro { get; set; } = new();

        // Lista restrita aos agendamentos do barbeiro logado.
        // Isso reforca a separacao de responsabilidade e evita que a view consulte dados fora do proprio contexto.
        public List<AgendamentoModel> Agendamentos { get; set; } = new();

        // O funcionario gerencia apenas os clientes que ele cadastrou.
        public List<ClienteModel> Clientes { get; set; } = new();
    }
}