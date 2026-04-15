using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;

namespace Navalha_Barbearia.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private static readonly List<ClienteModel> _clientes =
        [
            new ClienteModel
            {
                Id = 1,
                NomeCompleto = "Ana Lima",
                Telefone = "(11) 97777-3333",
                CPF = "123.456.789-00",
                Endereco = "Rua das Flores, 120",
                DataNascimento = new DateTime(1993, 4, 12),
                TipoAcesso = TipoAcessoEnum.Cliente,
                DataCadastro = DateTime.Now.AddMonths(-2),
                Ativo = true,
                BarbeiroQueCadastrou = new BarbeiroModel
                {
                    Id = 2,
                    NomeCompleto = "Carlos Funcionario",
                    Telefone = "(11) 98888-2222",
                    TipoAcesso = TipoAcessoEnum.Funcionario
                }
            },
            new ClienteModel
            {
                Id = 2,
                NomeCompleto = "Bianca Souza",
                Telefone = "(11) 96666-1111",
                CPF = "987.654.321-00",
                Endereco = "Av. Central, 455",
                DataNascimento = new DateTime(1989, 11, 3),
                TipoAcesso = TipoAcessoEnum.Cliente,
                DataCadastro = DateTime.Now.AddMonths(-1),
                Ativo = true,
                BarbeiroQueCadastrou = new BarbeiroModel
                {
                    Id = 1,
                    NomeCompleto = "Julia Admin",
                    Telefone = "(11) 99999-1111",
                    TipoAcesso = TipoAcessoEnum.Administrador
                }
            }
        ];

        public List<ClienteModel> ObterTodos()
        {
            return _clientes;
        }

        public List<ClienteModel> ObterPorBarbeiroId(int barbeiroId)
        {
            return _clientes.Where(x => x.BarbeiroQueCadastrou.Id == barbeiroId).ToList();
        }

        public ClienteModel? ObterPorId(int id)
        {
            return _clientes.FirstOrDefault(x => x.Id == id);
        }

        public ClienteModel? ObterPorCpf(string cpf)
        {
            return _clientes.FirstOrDefault(x => x.CPF.Equals(cpf, StringComparison.OrdinalIgnoreCase));
        }

        public ClienteModel Adicionar(ClienteModel cliente)
        {
            var proximoId = _clientes.Count == 0 ? 1 : _clientes.Max(x => x.Id) + 1;
            cliente.Id = proximoId;

            _clientes.Add(cliente);
            return cliente;
        }

        public ClienteModel Atualizar(ClienteModel cliente)
        {
            var indice = _clientes.FindIndex(x => x.Id == cliente.Id);
            if (indice < 0)
            {
                throw new KeyNotFoundException($"Cliente {cliente.Id} nao encontrado.");
            }

            _clientes[indice] = cliente;
            return cliente;
        }

        public ClienteModel Desativar(int id)
        {
            var cliente = ObterPorId(id) ?? throw new KeyNotFoundException($"Cliente {id} nao encontrado.");
            cliente.Ativo = false;
            return Atualizar(cliente);
        }

        public ClienteModel Ativar(int id)
        {
            var cliente = ObterPorId(id) ?? throw new KeyNotFoundException($"Cliente {id} nao encontrado.");
            cliente.Ativo = true;
            return Atualizar(cliente);
        }

        public void Excluir(int id)
        {
            var cliente = ObterPorId(id) ?? throw new KeyNotFoundException($"Cliente {id} nao encontrado.");
            _clientes.Remove(cliente);
        }
    }
}