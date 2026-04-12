using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;

namespace Navalha_Barbearia.Repositories
{
    public class BarbeiroRepository : IBarbeiroRepository
    {
        private static readonly List<BarbeiroModel> _barbeiros =
        [
            new BarbeiroModel
            {
                Id = 1,
                NomeCompleto = "Julia Admin",
                Telefone = "(11) 99999-1111",
                TipoAcesso = TipoAcessoEnum.Administrador,
                Procedimentos =
                [
                    new ProcedimentoModel
                    {
                        ProcedimentoEnum = ProcedimentoEnum.Corte,
                        Descricao = "Corte tradicional com acabamento na navalha.",
                        PrecoBase = 45.00m,
                        PrecoPorBarbeiro = 45.00m
                    },
                    new ProcedimentoModel
                    {
                        ProcedimentoEnum = ProcedimentoEnum.Barba,
                        Descricao = "Modelagem completa da barba com toalha quente.",
                        PrecoBase = 35.00m,
                        PrecoPorBarbeiro = 35.00m
                    }
                ],
                Clientes =
                [
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
                        Ativo = true
                    }
                ]
            },
            new BarbeiroModel
            {
                Id = 2,
                NomeCompleto = "Carlos Funcionario",
                Telefone = "(11) 98888-2222",
                TipoAcesso = TipoAcessoEnum.Funcionario,
                Procedimentos =
                [
                    new ProcedimentoModel
                    {
                        ProcedimentoEnum = ProcedimentoEnum.Corte,
                        Descricao = "Corte tradicional com acabamento na navalha.",
                        PrecoBase = 45.00m,
                        PrecoPorBarbeiro = 50.00m
                    },
                    new ProcedimentoModel
                    {
                        ProcedimentoEnum = ProcedimentoEnum.Sobrancelha,
                        Descricao = "Alinhamento e finalizacao de sobrancelha.",
                        PrecoBase = 20.00m,
                        PrecoPorBarbeiro = 22.00m
                    }
                ],
                Clientes =
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
                        Ativo = true
                    }
                ]
            }
        ];

        public List<BarbeiroModel> ObterTodos()
        {
            return _barbeiros;
        }

        public BarbeiroModel? ObterPorId(int id)
        {
            return _barbeiros.FirstOrDefault(x => x.Id == id);
        }

        public BarbeiroModel Adicionar(BarbeiroModel barbeiro)
        {
            var proximoId = _barbeiros.Count == 0 ? 1 : _barbeiros.Max(x => x.Id) + 1;
            barbeiro.Id = proximoId;

            _barbeiros.Add(barbeiro);
            return barbeiro;
        }

    }
}