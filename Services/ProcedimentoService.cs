using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Services
{
    public class ProcedimentoService : IProcedimentoService
    {
        private readonly IProcedimentoRepository _procedimentoRepository;
        private readonly IBarbeiroRepository _barbeiroRepository;

        public ProcedimentoService(IProcedimentoRepository procedimentoRepository, IBarbeiroRepository barbeiroRepository)
        {
            _procedimentoRepository = procedimentoRepository;
            _barbeiroRepository = barbeiroRepository;
        }

        public List<ProcedimentoModel> ObterTodos()
        {
            return _procedimentoRepository.ObterTodos();
        }

        public ProcedimentoModel? ObterPorId(int id)
        {
            return _procedimentoRepository.ObterPorId(id);
        }

        public ProcedimentoModel Criar(ProcedimentoModel procedimento, TipoAcessoEnum tipoAcessoSolicitante)
        {
            if (tipoAcessoSolicitante != TipoAcessoEnum.Administrador)
            {
                throw new UnauthorizedAccessException("Somente Administrador pode criar procedimentos.");
            }

            return _procedimentoRepository.Adicionar(procedimento);
        }

        public ProcedimentoModel AtualizarCatalogo(int id, ProcedimentoModel procedimentoAtualizado, TipoAcessoEnum tipoAcessoSolicitante)
        {
            if (tipoAcessoSolicitante != TipoAcessoEnum.Administrador)
            {
                throw new UnauthorizedAccessException("Somente Administrador pode atualizar descricao e preco base.");
            }

            var procedimentoCatalogo = _procedimentoRepository.ObterPorId(id)
                ?? throw new KeyNotFoundException($"Procedimento {id} nao encontrado.");

            procedimentoCatalogo.Nome = procedimentoAtualizado.Nome;
            procedimentoCatalogo.Descricao = procedimentoAtualizado.Descricao;
            procedimentoCatalogo.PrecoBase = procedimentoAtualizado.PrecoBase;

            foreach (var barbeiro in _barbeiroRepository.ObterTodos())
            {
                foreach (var procedimentoDoBarbeiro in barbeiro.Procedimentos.Where(x => x.Id == id))
                {
                    procedimentoDoBarbeiro.Nome = procedimentoCatalogo.Nome;
                    procedimentoDoBarbeiro.Descricao = procedimentoCatalogo.Descricao;
                    procedimentoDoBarbeiro.PrecoBase = procedimentoCatalogo.PrecoBase;
                }
            }

            return _procedimentoRepository.Atualizar(procedimentoCatalogo);
        }

        public void Excluir(int id, TipoAcessoEnum tipoAcessoSolicitante)
        {
            if (tipoAcessoSolicitante != TipoAcessoEnum.Administrador)
            {
                throw new UnauthorizedAccessException("Somente Administrador pode excluir procedimentos.");
            }

            _procedimentoRepository.Excluir(id);

            foreach (var barbeiro in _barbeiroRepository.ObterTodos())
            {
                barbeiro.Procedimentos.RemoveAll(x => x.Id == id);
            }
        }
    }
}