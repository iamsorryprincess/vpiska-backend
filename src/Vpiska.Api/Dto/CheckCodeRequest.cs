using System.ComponentModel.DataAnnotations;

namespace Vpiska.Api.Dto
{
    public sealed class CheckCodeRequest
    {
        [Required(ErrorMessage = "Телефон не заполнен")]
        [RegularExpression(@"^\d{10}\b$", ErrorMessage = "Номер телефона должен состоять из 10 цифр без кода")]
        public string Phone { get; set; }

        public int Code { get; set; }
    }
}