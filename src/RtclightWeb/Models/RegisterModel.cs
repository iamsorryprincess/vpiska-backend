using System.ComponentModel.DataAnnotations;

namespace RtclightWeb.Models
{
    public sealed class RegisterModel : LoginModel
    {
        [Compare(nameof(Password), ErrorMessage = "Password mismatch")]
        public string ConfirmPassword { get; set; }
    }
}