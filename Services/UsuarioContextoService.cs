using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Services;

/// <summary>
/// Implementacao unica para leitura/escrita do contexto de login na sessao.
/// Clean Code: nomes explicitos + metodos curtos com responsabilidade unica (SRP).
/// </summary>
public class UsuarioContextoService(IHttpContextAccessor httpContextAccessor) : IUsuarioContextoService
{
    private const string ChaveTipoAcesso = "Usuario.TipoAcesso";
    private const string ChaveIdBarbeiro = "Usuario.IdBarbeiro";
    private const string ChaveIdCliente = "Usuario.IdCliente";

    public void DefinirContextoLogin(TipoAcessoEnum tipoAcesso, int? idBarbeiro, int? idCliente)
    {
        var sessao = httpContextAccessor.HttpContext?.Session;
        if (sessao is null)
        {
            return;
        }

        sessao.SetInt32(ChaveTipoAcesso, (int)tipoAcesso);

        if (idBarbeiro.HasValue)
        {
            sessao.SetInt32(ChaveIdBarbeiro, idBarbeiro.Value);
        }
        else
        {
            sessao.Remove(ChaveIdBarbeiro);
        }

        if (idCliente.HasValue)
        {
            sessao.SetInt32(ChaveIdCliente, idCliente.Value);
        }
        else
        {
            sessao.Remove(ChaveIdCliente);
        }
    }

    public void LimparContextoLogin()
    {
        var sessao = httpContextAccessor.HttpContext?.Session;
        if (sessao is null)
        {
            return;
        }

        sessao.Remove(ChaveTipoAcesso);
        sessao.Remove(ChaveIdBarbeiro);
        sessao.Remove(ChaveIdCliente);
    }

    public TipoAcessoEnum? ObterTipoAcesso()
    {
        var valor = httpContextAccessor.HttpContext?.Session.GetInt32(ChaveTipoAcesso);
        return valor.HasValue ? (TipoAcessoEnum)valor.Value : null;
    }

    public int? ObterIdBarbeiro()
    {
        return httpContextAccessor.HttpContext?.Session.GetInt32(ChaveIdBarbeiro);
    }

    public int? ObterIdCliente()
    {
        return httpContextAccessor.HttpContext?.Session.GetInt32(ChaveIdCliente);
    }

    public bool UsuarioEstaLogado()
    {
        return ObterTipoAcesso().HasValue;
    }
}