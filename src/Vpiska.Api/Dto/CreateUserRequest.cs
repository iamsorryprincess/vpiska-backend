using System.ComponentModel.DataAnnotations;

namespace Vpiska.Api.Dto
{
    public sealed class CreateUserRequest
    {
        [Required(ErrorMessage = "Телефон не заполнен")]
        [RegularExpression(@"^\d{10}\b$", ErrorMessage = "Номер телефона должен состоять из 10 цифр без кода")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Имя не заполнено")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Пароль не заполнен")]
        [StringLength(6, ErrorMessage = "Допустимая длина 6 символов", MinimumLength = 6)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Поле для подтверждения пароля не заполнено")]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }
}