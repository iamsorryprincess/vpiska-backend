using System.ComponentModel.DataAnnotations;

namespace Vpiska.Api.Dto
{
    public sealed class LoginRequest
    {
        [Required(ErrorMessage = "Телефон не заполнен")]
        [RegularExpression(@"^\d{10}\b$", ErrorMessage = "Номер телефона должен состоять из 10 цифр без кода")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Пароль не заполнен")]
        [StringLength(6, ErrorMessage = "Допустимая длина 6 символов", MinimumLength = 6)]
        public string Password { get; set; }
    }
}