using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IBarbeiroRepository _barbeiroRepository;

        public ClienteService(IClienteRepository clienteRepository, IBarbeiroRepository barbeiroRepository)
        {
            _clienteRepository = clienteRepository;
            _barbeiroRepository = barbeiroRepository;
        }

        public List<ClienteModel> ObterTodosParaAdministrador(TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarAdministrador(tipoAcessoSolicitante);
            return _clienteRepository.ObterTodos();
        }

        public List<ClienteModel> ObterPorBarbeiro(int barbeiroId, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarFuncionarioOuAdministrador(tipoAcessoSolicitante);

            return tipoAcessoSolicitante == TipoAcessoEnum.Administrador
                ? _clienteRepository.ObterTodos()
                : _clienteRepository.ObterPorBarbeiroId(barbeiroId);
        }

        public ClienteModel? ObterPorCpfPublico(string cpf)
        {
            // Metodo publico apenas para auto preenchimento na Home.
            // Retorna somente cliente ativo para evitar uso de cadastro desativado.
            var cliente = _clienteRepository.ObterPorCpf(cpf);
            return cliente?.Ativo == true ? cliente : null;
        }

        public ClienteModel ObterPerfilCliente(int idCliente, TipoAcessoEnum tipoAcessoSolicitante)
        {
            if (tipoAcessoSolicitante != TipoAcessoEnum.Cliente)
            {
                throw new UnauthorizedAccessException("Somente Cliente pode visualizar o proprio cadastro pela area de cliente.");
            }

            return _clienteRepository.ObterPorId(idCliente)
                ?? throw new KeyNotFoundException($"Cliente {idCliente} nao encontrado.");
        }

        public ClienteModel CadastrarPorBarbeiro(int barbeiroId, ClienteModel cliente, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarFuncionarioOuAdministrador(tipoAcessoSolicitante);

            var barbeiro = _barbeiroRepository.ObterPorId(barbeiroId)
                ?? throw new KeyNotFoundException($"Barbeiro {barbeiroId} nao encontrado.");

            cliente.TipoAcesso = TipoAcessoEnum.Cliente;
            cliente.BarbeiroQueCadastrou = barbeiro;
            cliente.DataCadastro = DateTime.Now;
            cliente.Ativo = true;

            var novoCliente = _clienteRepository.Adicionar(cliente);
            barbeiro.Clientes.Add(novoCliente);

            return novoCliente;
        }

        public ClienteModel AtualizarPorBarbeiro(int clienteId, int barbeiroId, ClienteModel cliente, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarFuncionarioOuAdministrador(tipoAcessoSolicitante);

            var clienteExistente = _clienteRepository.ObterPorId(clienteId)
                ?? throw new KeyNotFoundException($"Cliente {clienteId} nao encontrado.");

            if (tipoAcessoSolicitante != TipoAcessoEnum.Administrador && clienteExistente.BarbeiroQueCadastrou.Id != barbeiroId)
            {
                throw new UnauthorizedAccessException("Barbeiro pode atualizar apenas os clientes que cadastrou.");
            }

            clienteExistente.NomeCompleto = cliente.NomeCompleto;
            clienteExistente.Telefone = cliente.Telefone;
            clienteExistente.CPF = cliente.CPF;
            clienteExistente.Endereco = cliente.Endereco;
            clienteExistente.DataNascimento = cliente.DataNascimento;

            return _clienteRepository.Atualizar(clienteExistente);
        }

        public ClienteModel DesativarPorBarbeiro(int clienteId, int barbeiroId, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarFuncionarioOuAdministrador(tipoAcessoSolicitante);

            var cliente = _clienteRepository.ObterPorId(clienteId)
                ?? throw new KeyNotFoundException($"Cliente {clienteId} nao encontrado.");

            if (tipoAcessoSolicitante != TipoAcessoEnum.Administrador && cliente.BarbeiroQueCadastrou.Id != barbeiroId)
            {
                throw new UnauthorizedAccessException("Barbeiro pode desativar apenas os clientes que cadastrou.");
            }

            return _clienteRepository.Desativar(clienteId);
        }

        public ClienteModel AtualizarPorAdministrador(int clienteId, ClienteModel cliente, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarAdministrador(tipoAcessoSolicitante);
            return AtualizarPorBarbeiro(clienteId, 0, cliente, TipoAcessoEnum.Administrador);
        }

        public ClienteModel DesativarPorAdministrador(int clienteId, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarAdministrador(tipoAcessoSolicitante);
            return _clienteRepository.Desativar(clienteId);
        }

        public ClienteModel AtivarPorAdministrador(int clienteId, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarAdministrador(tipoAcessoSolicitante);
            return _clienteRepository.Ativar(clienteId);
        }

        private static void ValidarAdministrador(TipoAcessoEnum tipoAcessoSolicitante)
        {
            if (tipoAcessoSolicitante != TipoAcessoEnum.Administrador)
            {
                throw new UnauthorizedAccessException("Somente Administrador pode executar essa acao.");
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