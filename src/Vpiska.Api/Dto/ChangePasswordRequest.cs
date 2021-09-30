using System.ComponentModel.DataAnnotations;

namespace Vpiska.Api.Dto
{
    public sealed class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Id обязателен")]
        public string Id { get; set; }
        
        [Required(ErrorMessage = "Пароль не заполнен")]
        [StringLength(6, ErrorMessage = "Допустимая длина 6 символов", MinimumLength = 6)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Поле для подтверждения пароля не заполнено")]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }
}