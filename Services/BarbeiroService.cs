using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Services
{
    public class BarbeiroService : IBarbeiroService
    {
        private readonly IBarbeiroRepository _barbeiroRepository;
        private readonly IProcedimentoRepository _procedimentoRepository;

        public BarbeiroService(IBarbeiroRepository barbeiroRepository, IProcedimentoRepository procedimentoRepository)
        {
            _barbeiroRepository = barbeiroRepository;
            _procedimentoRepository = procedimentoRepository;
        }

        public List<BarbeiroModel> ObterTodos()
        {
            // Responsabilidade unica: o service coordena as regras, mas nao conhece detalhes de persistencia.
            var barbeiros = _barbeiroRepository.ObterTodos();
            foreach (var barbeiro in barbeiros)
            {
                SincronizarEstruturasDeProcedimento(barbeiro);
            }

            return barbeiros;
        }

        public BarbeiroModel? ObterPorId(int id)
        {
            var barbeiro = _barbeiroRepository.ObterPorId(id);
            if (barbeiro is null)
            {
                return null;
            }

            SincronizarEstruturasDeProcedimento(barbeiro);
            return barbeiro;
        }

        public BarbeiroModel Criar(BarbeiroModel barbeiro)
        {
            // Codigo limpo: validacao simples e explicita antes de persistir o agregado.
            barbeiro.Procedimentos ??= new List<ProcedimentoModel>();
            barbeiro.RelacoesProcedimentos ??= new List<BarbeiroProcedimentoModel>();
            barbeiro.Clientes ??= new List<ClienteModel>();

            SincronizarEstruturasDeProcedimento(barbeiro);
            return _barbeiroRepository.Adicionar(barbeiro);
        }

        public BarbeiroModel Atualizar(BarbeiroModel barbeiro)
        {
            var barbeiroExistente = ObterBarbeiroObrigatorio(barbeiro.Id);

            // Mantemos as colecoes para nao apagar relacoes ja cadastradas na lista em memoria.
            barbeiro.Procedimentos = barbeiroExistente.Procedimentos;
            barbeiro.RelacoesProcedimentos = barbeiroExistente.RelacoesProcedimentos;
            barbeiro.Clientes = barbeiroExistente.Clientes;

            return _barbeiroRepository.Atualizar(barbeiro);
        }

        public void Excluir(int id)
        {
            _barbeiroRepository.Excluir(id);
        }

        public ProcedimentoModel AdicionarProcedimentoAoBarbeiro(int barbeiroId, int procedimentoId, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarGerenciamentoDeLista(tipoAcessoSolicitante);

            var barbeiro = ObterBarbeiroObrigatorio(barbeiroId);
            SincronizarEstruturasDeProcedimento(barbeiro);

            var procedimentoCatalogo = _procedimentoRepository.ObterPorId(procedimentoId)
                ?? throw new KeyNotFoundException($"Procedimento {procedimentoId} nao encontrado.");

            var relacaoExistente = barbeiro.RelacoesProcedimentos.FirstOrDefault(x => x.ProcedimentoId == procedimentoId);
            if (relacaoExistente is not null)
            {
                relacaoExistente.Ativo = true;
                relacaoExistente.AtualizadoEm = DateTime.Now;
                if (relacaoExistente.PrecoPorBarbeiro <= 0)
                {
                    relacaoExistente.PrecoPorBarbeiro = procedimentoCatalogo.PrecoBase;
                }

                SincronizarEstruturasDeProcedimento(barbeiro);
                return barbeiro.Procedimentos.First(x => x.Id == procedimentoId);
            }

            barbeiro.RelacoesProcedimentos.Add(new BarbeiroProcedimentoModel
            {
                BarbeiroId = barbeiro.Id,
                ProcedimentoId = procedimentoId,
                PrecoPorBarbeiro = procedimentoCatalogo.PrecoBase,
                Ativo = true,
                AtualizadoEm = DateTime.Now
            });

            SincronizarEstruturasDeProcedimento(barbeiro);

            return barbeiro.Procedimentos.First(x => x.Id == procedimentoId);
        }

        public ProcedimentoModel RemoverProcedimentoDoBarbeiro(int barbeiroId, int procedimentoId, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarGerenciamentoDeLista(tipoAcessoSolicitante);

            var barbeiro = ObterBarbeiroObrigatorio(barbeiroId);
            SincronizarEstruturasDeProcedimento(barbeiro);

            var relacao = barbeiro.RelacoesProcedimentos.FirstOrDefault(x => x.ProcedimentoId == procedimentoId && x.Ativo)
                ?? throw new KeyNotFoundException($"Procedimento {procedimentoId} nao esta vinculado ao barbeiro {barbeiroId}.");

            relacao.Ativo = false;
            relacao.AtualizadoEm = DateTime.Now;

            var procedimentoExistente = barbeiro.Procedimentos.First(x => x.Id == procedimentoId);
            SincronizarEstruturasDeProcedimento(barbeiro);

            return procedimentoExistente;
        }

        public ProcedimentoModel AtualizarPrecoPorBarbeiro(int barbeiroId, int procedimentoId, decimal precoPorBarbeiro, TipoAcessoEnum tipoAcessoSolicitante)
        {
            // Regra de negocio: o Administrador visualiza o preco, mas a alteracao do preco customizado e da area do Funcionario.
            if (tipoAcessoSolicitante != TipoAcessoEnum.Funcionario)
            {
                throw new UnauthorizedAccessException("Somente Funcionario pode atualizar PrecoPorBarbeiro.");
            }

            var barbeiro = ObterBarbeiroObrigatorio(barbeiroId);
            SincronizarEstruturasDeProcedimento(barbeiro);

            var relacao = barbeiro.RelacoesProcedimentos.FirstOrDefault(x => x.ProcedimentoId == procedimentoId && x.Ativo)
                ?? throw new KeyNotFoundException($"Procedimento {procedimentoId} nao esta vinculado ao barbeiro {barbeiroId}.");

            relacao.PrecoPorBarbeiro = precoPorBarbeiro;
            relacao.AtualizadoEm = DateTime.Now;

            SincronizarEstruturasDeProcedimento(barbeiro);
            var procedimento = barbeiro.Procedimentos.First(x => x.Id == procedimentoId);
            return procedimento;
        }

        private BarbeiroModel ObterBarbeiroObrigatorio(int barbeiroId)
        {
            return _barbeiroRepository.ObterPorId(barbeiroId)
                ?? throw new KeyNotFoundException($"Barbeiro {barbeiroId} nao encontrado.");
        }

        private void SincronizarEstruturasDeProcedimento(BarbeiroModel barbeiro)
        {
            barbeiro.Procedimentos ??= new List<ProcedimentoModel>();
            barbeiro.RelacoesProcedimentos ??= new List<BarbeiroProcedimentoModel>();

            if (barbeiro.RelacoesProcedimentos.Count == 0 && barbeiro.Procedimentos.Count > 0)
            {
                foreach (var procedimento in barbeiro.Procedimentos)
                {
                    barbeiro.RelacoesProcedimentos.Add(new BarbeiroProcedimentoModel
                    {
                        BarbeiroId = barbeiro.Id,
                        ProcedimentoId = procedimento.Id,
                        PrecoPorBarbeiro = procedimento.PrecoPorBarbeiro,
                        Ativo = true,
                        AtualizadoEm = DateTime.Now
                    });
                }
            }

            var relacoesAtivas = barbeiro.RelacoesProcedimentos
                .Where(x => x.Ativo)
                .ToDictionary(x => x.ProcedimentoId, x => x);

            barbeiro.Procedimentos.RemoveAll(x => !relacoesAtivas.ContainsKey(x.Id));

            foreach (var relacao in relacoesAtivas.Values)
            {
                var procedimentoCatalogo = _procedimentoRepository.ObterPorId(relacao.ProcedimentoId);
                if (procedimentoCatalogo is null)
                {
                    continue;
                }

                var procedimento = barbeiro.Procedimentos.FirstOrDefault(x => x.Id == relacao.ProcedimentoId);
                if (procedimento is null)
                {
                    procedimento = CopiarProcedimentoDoCatalogo(procedimentoCatalogo);
                    barbeiro.Procedimentos.Add(procedimento);
                }

                procedimento.Nome = procedimentoCatalogo.Nome;
                procedimento.Descricao = procedimentoCatalogo.Descricao;
                procedimento.PrecoBase = procedimentoCatalogo.PrecoBase;
                procedimento.PrecoPorBarbeiro = relacao.PrecoPorBarbeiro;
            }
        }

        private static ProcedimentoModel CopiarProcedimentoDoCatalogo(ProcedimentoModel procedimentoCatalogo)
        {
            return new ProcedimentoModel
            {
                Id = procedimentoCatalogo.Id,
                Nome = procedimentoCatalogo.Nome,
                Descricao = procedimentoCatalogo.Descricao,
                PrecoBase = procedimentoCatalogo.PrecoBase
            };
        }

        private static void ValidarGerenciamentoDeLista(TipoAcessoEnum tipoAcessoSolicitante)
        {
            // Aberto para Barbeiro Funcionario e Administrador, seguindo o principio de menor surpresa.
            if (tipoAcessoSolicitante is not (TipoAcessoEnum.Funcionario or TipoAcessoEnum.Administrador))
            {
                throw new UnauthorizedAccessException("Somente Funcionario ou Administrador pode alterar a lista de procedimentos do barbeiro.");
            }
        }
    }
}