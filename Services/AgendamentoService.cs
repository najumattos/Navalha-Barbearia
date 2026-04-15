using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Services
{
    public class AgendamentoService : IAgendamentoService
    {
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly IBarbeiroRepository _barbeiroRepository;
        private readonly IProcedimentoRepository _procedimentoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly ISlotHorarioService _slotHorarioService;
        private readonly IAgendamentoValidacaoService _agendamentoValidacaoService;

        public AgendamentoService(
            IAgendamentoRepository agendamentoRepository,
            IBarbeiroRepository barbeiroRepository,
            IProcedimentoRepository procedimentoRepository,
            IClienteRepository clienteRepository,
            ISlotHorarioService slotHorarioService,
            IAgendamentoValidacaoService agendamentoValidacaoService)
        {
            _agendamentoRepository = agendamentoRepository;
            _barbeiroRepository = barbeiroRepository;
            _procedimentoRepository = procedimentoRepository;
            _clienteRepository = clienteRepository;
            _slotHorarioService = slotHorarioService;
            _agendamentoValidacaoService = agendamentoValidacaoService;
        }

        public List<AgendamentoModel> ObterTodosParaAdministrador(TipoAcessoEnum tipoAcessoSolicitante)
        {
            // Regra clara: apenas Administrador enxerga a agenda completa.
            if (tipoAcessoSolicitante != TipoAcessoEnum.Administrador)
            {
                throw new UnauthorizedAccessException("Somente Administrador pode visualizar todos os agendamentos.");
            }

            // O service concentra a regra e deixa o repository apenas com persistencia simples.
            var agendamentos = _agendamentoRepository.ObterTodos();
            
            // Aplica transicao automatica de status para agendamentos proximos (faltam 30 min).
            foreach (var agendamento in agendamentos)
            {
                AplicarTransicaoDeStatusAutomatica(agendamento);
            }
            
            return agendamentos;
        }

        public List<AgendamentoModel> ObterPorBarbeiroId(int barbeiroId, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarFuncionarioOuAdministrador(tipoAcessoSolicitante);

            // O barbeiro acessa apenas o proprio conjunto de agendamentos.
            var agendamentos = tipoAcessoSolicitante == TipoAcessoEnum.Administrador
                ? _agendamentoRepository.ObterTodos()
                : _agendamentoRepository.ObterPorBarbeiroId(barbeiroId);
            
            // Aplica transicao automatica de status para agendamentos proximos (faltam 30 min).
            foreach (var agendamento in agendamentos)
            {
                AplicarTransicaoDeStatusAutomatica(agendamento);
            }
            
            return agendamentos;
        }

        public List<AgendamentoModel> ObterPorCpfCliente(string cpf, TipoAcessoEnum tipoAcessoSolicitante)
        {
            if (tipoAcessoSolicitante != TipoAcessoEnum.Cliente)
            {
                throw new UnauthorizedAccessException("Somente Cliente pode visualizar agendamentos por CPF na area de cliente.");
            }

            var agendamentos = _agendamentoRepository.ObterPorCpfCliente(cpf);
            
            // Aplica transicao automatica de status para agendamentos proximos (faltam 30 min).
            foreach (var agendamento in agendamentos)
            {
                AplicarTransicaoDeStatusAutomatica(agendamento);
            }
            
            return agendamentos;
        }

        public List<AgendamentoModel> ObterHistoricoPorCpfParaEquipe(string cpf, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarFuncionarioOuAdministrador(tipoAcessoSolicitante);

            // A tela de detalhes do cliente precisa mostrar o historico completo, independente do barbeiro que atendeu.
            var agendamentos = _agendamentoRepository.ObterPorCpfCliente(cpf)
                .OrderByDescending(x => x.DataHora)
                .ToList();
            
            // Aplica transicao automatica de status para agendamentos proximos (faltam 30 min).
            foreach (var agendamento in agendamentos)
            {
                AplicarTransicaoDeStatusAutomatica(agendamento);
            }
            
            return agendamentos;
        }

        public AgendamentoModel? ObterPorId(int idAgendamento, int barbeiroIdSolicitante, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarFuncionarioOuAdministrador(tipoAcessoSolicitante);

            var agendamento = _agendamentoRepository.ObterPorId(idAgendamento);
            if (agendamento is null)
            {
                return null;
            }

            if (tipoAcessoSolicitante == TipoAcessoEnum.Funcionario && agendamento.Barbeiro.Id != barbeiroIdSolicitante)
            {
                throw new UnauthorizedAccessException("Funcionario pode visualizar apenas os proprios agendamentos.");
            }

            // Aplica transicao automatica de status para agendamentos proximos (faltam 30 min).
            AplicarTransicaoDeStatusAutomatica(agendamento);

            return agendamento;
        }

        public AgendamentoModel Criar(AgendamentoModel agendamento)
        {
            // Guard clause leve para manter o estado inicial padrao do fluxo.
            agendamento.StatusAgendamentoEnum = Enum.IsDefined(agendamento.StatusAgendamentoEnum)
                ? agendamento.StatusAgendamentoEnum
                : StatusAgendamentoEnum.Pendente;

            var slotValidado = _agendamentoValidacaoService.ValidarCriacaoComSlotLivre(agendamento);
            agendamento.DataHora = slotValidado.Inicio;

            // Regra de negocio centralizada no service:
            // o preco final do agendamento precisa seguir o PrecoPorBarbeiro do profissional selecionado.
            var barbeiroId = agendamento.Barbeiro?.Id;
            if (!barbeiroId.HasValue || barbeiroId.Value <= 0)
            {
                throw new ArgumentException("Barbeiro invalido para o agendamento.");
            }

            var cpfCliente = agendamento.Cliente?.CPF;
            if (string.IsNullOrWhiteSpace(cpfCliente))
            {
                throw new ArgumentException("CPF do cliente e obrigatorio para agendamento.");
            }

            var cliente = _clienteRepository.ObterPorCpf(cpfCliente)
                ?? throw new KeyNotFoundException($"Cliente com CPF {cpfCliente} nao encontrado.");

            var barbeiro = _barbeiroRepository.ObterPorId(barbeiroId.Value)
                ?? throw new KeyNotFoundException($"Barbeiro {barbeiroId.Value} nao encontrado.");

            var procedimentoId = (int)agendamento.Procedimento;
            if (!ProcedimentoEstaDisponivelParaBarbeiro(barbeiro, procedimentoId))
            {
                throw new InvalidOperationException("O procedimento selecionado nao esta ativo para o barbeiro informado.");
            }

            var precoDoBarbeiro = ObterPrecoDoBarbeiro(barbeiro, procedimentoId);
            if (precoDoBarbeiro.HasValue)
            {
                agendamento.Preco = precoDoBarbeiro.Value;
            }
            else
            {
                // Fallback seguro: usa preco base do catalogo quando o barbeiro ainda nao tiver o procedimento na lista.
                var procedimentoCatalogo = _procedimentoRepository.ObterPorId((int)agendamento.Procedimento)
                    ?? throw new KeyNotFoundException($"Procedimento {agendamento.Procedimento} nao encontrado no catalogo.");

                agendamento.Preco = procedimentoCatalogo.PrecoBase;
            }

            // Mantemos a entidade relacionada completa para preservar consistencia entre as camadas.
            agendamento.Barbeiro = barbeiro;
            agendamento.Cliente = cliente;

            var agendamentoCriado = _agendamentoRepository.Adicionar(agendamento);
            _slotHorarioService.OcuparSlot(slotValidado.Id);

            return agendamentoCriado;
        }

        private static decimal? ObterPrecoDoBarbeiro(BarbeiroModel barbeiro, int procedimentoId)
        {
            var possuiRelacoes = barbeiro.RelacoesProcedimentos.Any();

            var relacaoAtiva = barbeiro.RelacoesProcedimentos
                .FirstOrDefault(x => x.ProcedimentoId == procedimentoId && x.Ativo);

            if (relacaoAtiva is not null)
            {
                return relacaoAtiva.PrecoPorBarbeiro;
            }

            if (possuiRelacoes)
            {
                return null;
            }

            var procedimentoLegado = barbeiro.Procedimentos.FirstOrDefault(x => x.Id == procedimentoId);
            return procedimentoLegado?.PrecoPorBarbeiro;
        }

        private static bool ProcedimentoEstaDisponivelParaBarbeiro(BarbeiroModel barbeiro, int procedimentoId)
        {
            if (procedimentoId <= 0)
            {
                return false;
            }

            var possuiRelacoes = barbeiro.RelacoesProcedimentos.Any();
            if (possuiRelacoes)
            {
                return barbeiro.RelacoesProcedimentos.Any(x => x.ProcedimentoId == procedimentoId && x.Ativo);
            }

            return barbeiro.Procedimentos.Any(x => x.Id == procedimentoId);
        }

        public AgendamentoModel AtualizarDoFuncionario(int idAgendamento, int barbeiroIdSolicitante, AgendamentoModel agendamento, TipoAcessoEnum tipoAcessoSolicitante)
        {
            // Regra de negocio: somente Funcionario altera os agendamentos vinculados ao proprio id.
            if (tipoAcessoSolicitante is not (TipoAcessoEnum.Funcionario or TipoAcessoEnum.Administrador))
            {
                throw new UnauthorizedAccessException("Somente Funcionario ou Administrador pode alterar agendamentos.");
            }

            var agendamentoExistente = _agendamentoRepository.ObterPorId(idAgendamento)
                ?? throw new KeyNotFoundException($"Agendamento {idAgendamento} nao encontrado.");

            if (tipoAcessoSolicitante == TipoAcessoEnum.Funcionario && agendamentoExistente.Barbeiro.Id != barbeiroIdSolicitante)
            {
                throw new UnauthorizedAccessException("Funcionario pode alterar apenas os agendamentos vinculados ao seu id.");
            }

            // Pratica de codigo limpo: preserva dados que nao devem mudar nessa tela e atualiza apenas o necessario.
            agendamentoExistente.DataHora = agendamento.DataHora;
            agendamentoExistente.StatusAgendamentoEnum = agendamento.StatusAgendamentoEnum;
            agendamentoExistente.Observacao = agendamento.Observacao;

            return _agendamentoRepository.Atualizar(agendamentoExistente);
        }

        public AgendamentoModel AtualizarStatus(int idAgendamento, StatusAgendamentoEnum status, TipoAcessoEnum tipoAcessoSolicitante, int? barbeiroIdSolicitante = null, string? cpfClienteSolicitante = null)
        {
            var agendamento = _agendamentoRepository.ObterPorId(idAgendamento)
                ?? throw new KeyNotFoundException($"Agendamento {idAgendamento} nao encontrado.");

            // Regra centralizada: cada perfil pode mudar apenas o proprio conjunto de agendamentos.
            if (tipoAcessoSolicitante == TipoAcessoEnum.Funcionario)
            {
                if (!barbeiroIdSolicitante.HasValue)
                {
                    throw new ArgumentException("Id do barbeiro e obrigatorio para atualizar o status.");
                }

                if (agendamento.Barbeiro.Id != barbeiroIdSolicitante.Value)
                {
                    throw new UnauthorizedAccessException("Funcionario pode alterar apenas os proprios agendamentos.");
                }
            }
            else if (tipoAcessoSolicitante == TipoAcessoEnum.Cliente)
            {
                if (string.IsNullOrWhiteSpace(cpfClienteSolicitante))
                {
                    throw new ArgumentException("CPF do cliente e obrigatorio para atualizar o status.");
                }

                if (!agendamento.Cliente.CPF.Equals(cpfClienteSolicitante, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException("Cliente pode alterar apenas o proprio agendamento.");
                }
            }
            else if (tipoAcessoSolicitante != TipoAcessoEnum.Administrador)
            {
                throw new UnauthorizedAccessException("Acesso permitido apenas para Admin, Funcionario ou Cliente.");
            }

            agendamento.StatusAgendamentoEnum = status;
            return _agendamentoRepository.Atualizar(agendamento);
        }

        public AgendamentoModel AtualizarStatusDoCliente(int idAgendamento, string cpfClienteSolicitante, StatusAgendamentoEnum status, TipoAcessoEnum tipoAcessoSolicitante)
        {
            if (tipoAcessoSolicitante != TipoAcessoEnum.Cliente)
            {
                throw new UnauthorizedAccessException("Somente Cliente pode alterar o status do proprio agendamento.");
            }

            var agendamento = _agendamentoRepository.ObterPorId(idAgendamento)
                ?? throw new KeyNotFoundException($"Agendamento {idAgendamento} nao encontrado.");

            if (!agendamento.Cliente.CPF.Equals(cpfClienteSolicitante, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Cliente pode alterar apenas os agendamentos vinculados ao proprio CPF.");
            }

            agendamento.StatusAgendamentoEnum = status;
            return _agendamentoRepository.Atualizar(agendamento);
        }

        public void Excluir(int idAgendamento, int barbeiroIdSolicitante, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarFuncionarioOuAdministrador(tipoAcessoSolicitante);

            var agendamento = _agendamentoRepository.ObterPorId(idAgendamento)
                ?? throw new KeyNotFoundException($"Agendamento {idAgendamento} nao encontrado.");

            if (tipoAcessoSolicitante == TipoAcessoEnum.Funcionario && agendamento.Barbeiro.Id != barbeiroIdSolicitante)
            {
                throw new UnauthorizedAccessException("Funcionario pode excluir apenas os proprios agendamentos.");
            }

            _agendamentoRepository.Excluir(idAgendamento);
        }

        /// <summary>
        /// Verifica se o agendamento esta proximo ao seu horario (faltam 30 minutos ou menos).
        /// Se estiver com status Agendado, transiciona para AguardandoConfirmacaoCliente.
        /// Centraliza a logica de reconfirmacao para melhorar a experiencia do cliente antes do atendimento.
        /// </summary>
        private void AplicarTransicaoDeStatusAutomatica(AgendamentoModel agendamento)
        {
            // Guard clause: apenas agendamentos com status "Agendado" sao candidatos a transicao.
            if (agendamento.StatusAgendamentoEnum != StatusAgendamentoEnum.Agendado)
            {
                return;
            }

            // Calcula o tempo restante ate o horario do agendamento.
            var tempoRestante = agendamento.DataHora - DateTime.Now;
            
            // Se faltam 30 minutos ou menos, transiciona para AguardandoConfirmacaoCliente.
            if (tempoRestante <= TimeSpan.FromMinutes(30))
            {
                agendamento.StatusAgendamentoEnum = StatusAgendamentoEnum.AguardandoConfirmacaoCliente;
                _agendamentoRepository.Atualizar(agendamento);
            }
        }

        private static void ValidarFuncionarioOuAdministrador(TipoAcessoEnum tipoAcessoSolicitante)
        {
            if (tipoAcessoSolicitante is not (TipoAcessoEnum.Funcionario or TipoAcessoEnum.Administrador))
            {
                throw new UnauthorizedAccessException("Acesso permitido apenas para Funcionario ou Administrador.");
            }
        }
    }
}