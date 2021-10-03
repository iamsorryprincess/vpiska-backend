namespace Vpiska.Domain.Models
{
    public sealed class User
    {
        public string Id { get; set; }
        
        public string Name { get; set; }

        public string PhoneCode { get; set; }

        public string Phone { get; set; }

        public string ImageUrl { get; set; }

        public string Password { get; set; }

        public int VerificationCode { get; set; }
    }
}