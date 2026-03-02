using System.ComponentModel.DataAnnotations;

namespace RecursosHumanos.Models.ViewModels.Login
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Debe ingresar el usuario")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar la contraseña")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}