using Navalha_Barbearia.Enums;

namespace Navalha_Barbearia.Services.Interfaces
{
    /// <summary>
    /// Abstracao de contexto de usuario logado.
    /// SOLID (DIP): controllers e views dependem de uma interface, nao de sessao diretamente.
    /// </summary>
    public interface IUsuarioContextoService
    {
        void DefinirContextoLogin(TipoAcessoEnum tipoAcesso, int? idBarbeiro, int? idCliente);
        void LimparContextoLogin();
        TipoAcessoEnum? ObterTipoAcesso();
        int? ObterIdBarbeiro();
        int? ObterIdCliente();
        bool UsuarioEstaLogado();
    }
}