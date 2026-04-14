using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Repositories.Interfaces;

namespace Navalha_Barbearia.Repositories
{
    public class ProcedimentoRepository : IProcedimentoRepository
    {
        private static readonly List<ProcedimentoModel> _procedimentoLista =
        [
            new ProcedimentoModel
            {
                Id = 1,
                Nome = "Corte",
                Descricao = "Corte tradicional com acabamento na navalha.",
                PrecoBase = 45.00m
            },
            new ProcedimentoModel
            {
                Id = 2,
                Nome = "Barba",
                Descricao = "Modelagem completa da barba com toalha quente.",
                PrecoBase = 35.00m
            }
        ];

        public List<ProcedimentoModel> ObterTodos()
        {
            return _procedimentoLista;
        }

        public ProcedimentoModel? ObterPorId(int id)
        {
            return _procedimentoLista.FirstOrDefault(x => x.Id == id);
        }

        public ProcedimentoModel Adicionar(ProcedimentoModel procedimento)
        {
            if (procedimento.Id <= 0)
            {
                procedimento.Id = _procedimentoLista.Count == 0 ? 1 : _procedimentoLista.Max(x => x.Id) + 1;
            }
            else
            {
                _procedimentoLista.RemoveAll(x => x.Id == procedimento.Id);
            }

            _procedimentoLista.Add(procedimento);
            return procedimento;
        }

        public ProcedimentoModel Atualizar(ProcedimentoModel procedimento)
        {
            return Adicionar(procedimento);
        }

        public void Excluir(int id)
        {
            var removidos = _procedimentoLista.RemoveAll(x => x.Id == id);
            if (removidos == 0)
            {
                throw new KeyNotFoundException($"Procedimento {id} nao encontrado.");
            }
        }

    }
}