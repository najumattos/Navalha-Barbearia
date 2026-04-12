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
                ProcedimentoEnum = ProcedimentoEnum.Corte,
                Descricao = "Corte tradicional com acabamento na navalha.",
                PrecoBase = 45.00m
            },
            new ProcedimentoModel
            {
                ProcedimentoEnum = ProcedimentoEnum.Barba,
                Descricao = "Modelagem completa da barba com toalha quente.",
                PrecoBase = 35.00m
            }
        ];

        public List<ProcedimentoModel> ObterTodos()
        {
            return _procedimentoLista;
        }

        public ProcedimentoModel? ObterPorTipo(ProcedimentoEnum procedimentoEnum)
        {
            return _procedimentoLista.FirstOrDefault(x => x.ProcedimentoEnum == procedimentoEnum);
        }

        public ProcedimentoModel Adicionar(ProcedimentoModel procedimento)
        {
            _procedimentoLista.RemoveAll(x => x.ProcedimentoEnum == procedimento.ProcedimentoEnum);

            _procedimentoLista.Add(procedimento);
            return procedimento;
        }

    }
}