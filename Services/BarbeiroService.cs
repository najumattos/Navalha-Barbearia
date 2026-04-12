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
            return _barbeiroRepository.ObterTodos();
        }

        public BarbeiroModel? ObterPorId(int id)
        {
            return _barbeiroRepository.ObterPorId(id);
        }

        public BarbeiroModel Criar(BarbeiroModel barbeiro)
        {
            // Codigo limpo: validacao simples e explicita antes de persistir o agregado.
            barbeiro.Procedimentos ??= new List<ProcedimentoModel>();
            barbeiro.Clientes ??= new List<ClienteModel>();
            return _barbeiroRepository.Adicionar(barbeiro);
        }

        public BarbeiroModel Atualizar(BarbeiroModel barbeiro)
        {
            var barbeiroExistente = ObterBarbeiroObrigatorio(barbeiro.Id);

            // Mantemos as colecoes para nao apagar relacoes ja cadastradas na lista em memoria.
            barbeiro.Procedimentos = barbeiroExistente.Procedimentos;
            barbeiro.Clientes = barbeiroExistente.Clientes;

            return _barbeiroRepository.Atualizar(barbeiro);
        }

        public void Excluir(int id)
        {
            _barbeiroRepository.Excluir(id);
        }

        public ProcedimentoModel AdicionarProcedimentoAoBarbeiro(int barbeiroId, ProcedimentoEnum procedimentoEnum, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarGerenciamentoDeLista(tipoAcessoSolicitante);

            var barbeiro = ObterBarbeiroObrigatorio(barbeiroId);
            var procedimentoCatalogo = _procedimentoRepository.ObterPorTipo(procedimentoEnum)
                ?? throw new KeyNotFoundException($"Procedimento {procedimentoEnum} nao encontrado.");

            var procedimentoExistente = barbeiro.Procedimentos.FirstOrDefault(x => x.ProcedimentoEnum == procedimentoEnum);
            if (procedimentoExistente is not null)
            {
                return procedimentoExistente;
            }

            var procedimentoNovo = CopiarProcedimentoDoCatalogo(procedimentoCatalogo);
            barbeiro.Procedimentos.Add(procedimentoNovo);

            return procedimentoNovo;
        }

        public ProcedimentoModel RemoverProcedimentoDoBarbeiro(int barbeiroId, ProcedimentoEnum procedimentoEnum, TipoAcessoEnum tipoAcessoSolicitante)
        {
            ValidarGerenciamentoDeLista(tipoAcessoSolicitante);

            var barbeiro = ObterBarbeiroObrigatorio(barbeiroId);
            var procedimentoExistente = barbeiro.Procedimentos.FirstOrDefault(x => x.ProcedimentoEnum == procedimentoEnum)
                ?? throw new KeyNotFoundException($"Procedimento {procedimentoEnum} nao esta vinculado ao barbeiro {barbeiroId}.");

            barbeiro.Procedimentos.Remove(procedimentoExistente);
            return procedimentoExistente;
        }

        public ProcedimentoModel AtualizarPrecoPorBarbeiro(int barbeiroId, ProcedimentoEnum procedimentoEnum, decimal precoPorBarbeiro, TipoAcessoEnum tipoAcessoSolicitante)
        {
            // Regra de negocio: o Administrador visualiza o preco, mas a alteracao do preco customizado e da area do Funcionario.
            if (tipoAcessoSolicitante != TipoAcessoEnum.Funcionario)
            {
                throw new UnauthorizedAccessException("Somente Funcionario pode atualizar PrecoPorBarbeiro.");
            }

            var barbeiro = ObterBarbeiroObrigatorio(barbeiroId);
            var procedimento = barbeiro.Procedimentos.FirstOrDefault(x => x.ProcedimentoEnum == procedimentoEnum)
                ?? throw new KeyNotFoundException($"Procedimento {procedimentoEnum} nao esta vinculado ao barbeiro {barbeiroId}.");

            procedimento.PrecoPorBarbeiro = precoPorBarbeiro;
            return procedimento;
        }

        private BarbeiroModel ObterBarbeiroObrigatorio(int barbeiroId)
        {
            return _barbeiroRepository.ObterPorId(barbeiroId)
                ?? throw new KeyNotFoundException($"Barbeiro {barbeiroId} nao encontrado.");
        }

        private static ProcedimentoModel CopiarProcedimentoDoCatalogo(ProcedimentoModel procedimentoCatalogo)
        {
            // Copia explicita para manter o preco customizado independente entre barbeiros.
            return new ProcedimentoModel
            {
                ProcedimentoEnum = procedimentoCatalogo.ProcedimentoEnum,
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