using Navalha_Barbearia.Enums;

namespace Navalha_Barbearia.Models
{
    public class LoginModel
    {
        public int Id { get; set; }

        // Login pode apontar para barbeiro ou cliente, dependendo do tipo de acesso.
        public int? IdBarbeiro { get; set; }

        public int? IdCliente { get; set; }

        public TipoAcessoEnum TipoAcessoEnum { get; set; }

        public string Senha { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}