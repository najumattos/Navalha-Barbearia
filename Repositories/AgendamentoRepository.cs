using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;

namespace Navalha_Barbearia.Repositories
{
    public class AgendamentoRepository : IAgendamentoRepository
    {
        // Lista em memoria: pratica util para desenvolvimento e testes sem banco de dados.
        private static readonly List<AgendamentoModel> _agendamentos =
        [
            new AgendamentoModel
            {
                IdAgendamento = 1,
                Cliente = new ClienteModel
                {
                    Id = 1,
                    NomeCompleto = "Ana Lima",
                    Telefone = "(11) 97777-3333",
                    CPF = "123.456.789-00",
                    Endereco = "Rua das Flores, 120",
                    DataNascimento = new DateTime(1993, 4, 12),
                    GeneroEnum = GeneroEnum.Feminino,
                    TipoAcesso = TipoAcessoEnum.Cliente,
                    DataCadastro = DateTime.Now.AddMonths(-2),
                    Ativo = true
                },
                // O agendamento referencia um barbeiro real do dominio, preservando a relacao entre as entidades.
                Barbeiro = new BarbeiroModel
                {
                    Id = 2,
                    NomeCompleto = "Carlos Funcionario",
                    Telefone = "(11) 98888-2222",
                    TipoAcesso = TipoAcessoEnum.Funcionario
                },
                Procedimento = ProcedimentoEnum.Corte,
                DataHora = DateTime.Now.AddDays(1),
                StatusAgendamentoEnum = StatusAgendamentoEnum.Agendado,
                Preco = 50.00m,
                Observacao = "Cliente prefere horario da manha."
            },
            new AgendamentoModel
            {
                IdAgendamento = 2,
                Cliente = new ClienteModel
                {
                    Id = 2,
                    NomeCompleto = "Bianca Souza",
                    Telefone = "(11) 96666-1111",
                    CPF = "987.654.321-00",
                    Endereco = "Av. Central, 455",
                    DataNascimento = new DateTime(1989, 11, 3),
                    GeneroEnum = GeneroEnum.Feminino,
                    TipoAcesso = TipoAcessoEnum.Cliente,
                    DataCadastro = DateTime.Now.AddMonths(-1),
                    Ativo = true
                },
                // Mantemos a relacao real entre cliente, barbeiro e procedimento para o seed seguir o mesmo dominio da aplicacao.
                Barbeiro = new BarbeiroModel
                {
                    Id = 1,
                    NomeCompleto = "Julia Admin",
                    Telefone = "(11) 99999-1111",
                    TipoAcesso = TipoAcessoEnum.Administrador
                },
                Procedimento = ProcedimentoEnum.Barba,
                DataHora = DateTime.Now.AddDays(2),
                StatusAgendamentoEnum = StatusAgendamentoEnum.Pendente,
                Preco = 35.00m,
                Observacao = "Agendamento criado para o CPF solicitado."
            }
        ];

        public List<AgendamentoModel> ObterTodos()
        {
            return _agendamentos;
        }

        public List<AgendamentoModel> ObterPorBarbeiroId(int barbeiroId)
        {
            return _agendamentos.Where(x => x.Barbeiro.Id == barbeiroId).ToList();
        }

        public List<AgendamentoModel> ObterPorCpfCliente(string cpf)
        {
            return _agendamentos.Where(x => x.Cliente.CPF.Equals(cpf, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public AgendamentoModel? ObterPorId(int idAgendamento)
        {
            return _agendamentos.FirstOrDefault(x => x.IdAgendamento == idAgendamento);
        }

        public AgendamentoModel Adicionar(AgendamentoModel agendamento)
        {
            var proximoId = _agendamentos.Count == 0 ? 1 : _agendamentos.Max(x => x.IdAgendamento) + 1;
            agendamento.IdAgendamento = proximoId;

            _agendamentos.Add(agendamento);
            return agendamento;
        }

        public AgendamentoModel Atualizar(AgendamentoModel agendamento)
        {
            var indice = _agendamentos.FindIndex(x => x.IdAgendamento == agendamento.IdAgendamento);
            if (indice < 0)
            {
                throw new KeyNotFoundException($"Agendamento {agendamento.IdAgendamento} nao encontrado.");
            }

            // Atualizacao in-memory direta: simples, previsivel e suficiente para o cenario atual.
            _agendamentos[indice] = agendamento;
            return agendamento;
        }

        public void Excluir(int idAgendamento)
        {
            var agendamento = ObterPorId(idAgendamento) ?? throw new KeyNotFoundException($"Agendamento {idAgendamento} nao encontrado.");
            _agendamentos.Remove(agendamento);
        }
    }
}