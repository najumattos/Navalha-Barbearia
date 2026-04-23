using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Services;

public class ProcedimentoService(IProcedimentoRepository procedimentoRepository, IBarbeiroRepository barbeiroRepository) : IProcedimentoService
{
    public List<ProcedimentoModel> ObterTodos()
    {
        return procedimentoRepository.ObterTodos();
    }

    public ProcedimentoModel? ObterPorId(int id)
    {
        return procedimentoRepository.ObterPorId(id);
    }

    public ProcedimentoModel Criar(ProcedimentoModel procedimento, TipoAcessoEnum tipoAcessoSolicitante)
    {
        if (tipoAcessoSolicitante != TipoAcessoEnum.Administrador)
        {
            throw new UnauthorizedAccessException("Somente Administrador pode criar procedimentos.");
        }

        return procedimentoRepository.Adicionar(procedimento);
    }

    public ProcedimentoModel AtualizarCatalogo(int id, ProcedimentoModel procedimentoAtualizado, TipoAcessoEnum tipoAcessoSolicitante)
    {
        if (tipoAcessoSolicitante != TipoAcessoEnum.Administrador)
        {
            throw new UnauthorizedAccessException("Somente Administrador pode atualizar descricao e preco base.");
        }

        var procedimentoCatalogo = procedimentoRepository.ObterPorId(id)
            ?? throw new KeyNotFoundException($"Procedimento {id} nao encontrado.");

        procedimentoCatalogo.Nome = procedimentoAtualizado.Nome;
        procedimentoCatalogo.Descricao = procedimentoAtualizado.Descricao;
        procedimentoCatalogo.PrecoBase = procedimentoAtualizado.PrecoBase;

        foreach (var barbeiro in barbeiroRepository.ObterTodos())
        {
            foreach (var procedimentoDoBarbeiro in barbeiro.Procedimentos.Where(x => x.Id == id))
            {
                procedimentoDoBarbeiro.Nome = procedimentoCatalogo.Nome;
                procedimentoDoBarbeiro.Descricao = procedimentoCatalogo.Descricao;
                procedimentoDoBarbeiro.PrecoBase = procedimentoCatalogo.PrecoBase;
            }
        }

        return procedimentoRepository.Atualizar(procedimentoCatalogo);
    }

    public void Excluir(int id, TipoAcessoEnum tipoAcessoSolicitante)
    {
        if (tipoAcessoSolicitante != TipoAcessoEnum.Administrador)
        {
            throw new UnauthorizedAccessException("Somente Administrador pode excluir procedimentos.");
        }

        procedimentoRepository.Excluir(id);

        foreach (var barbeiro in barbeiroRepository.ObterTodos())
        {
            barbeiro.Procedimentos.RemoveAll(x => x.Id == id);
        }
    }
}