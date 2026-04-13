namespace Navalha_Barbearia.Enums
{
    // Identificadores explicitos facilitam persistencia futura e deixam a regra mais previsivel.
    public enum StatusAgendamentoEnum
    {
        Cancelado = 1,
        Agendado = 2,
        Concluido = 3,
        Pendente = 4, 
        AguardandoConfirmacaoBarbeiro = 5,
        AguardandoConfirmacaoCliente = 6
    }
}