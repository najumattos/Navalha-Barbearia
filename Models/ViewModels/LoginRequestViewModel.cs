using System.ComponentModel.DataAnnotations;

namespace Navalha_Barbearia.Models.ViewModels
{
    public class LoginRequestViewModel
    {
        [Required(ErrorMessage = "Informe e-mail ou CPF.")]
        public string Identificador { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a senha.")]
        public string Senha { get; set; } = string.Empty;
    }
}