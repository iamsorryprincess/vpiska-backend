using System;

namespace Vpiska.Domain.Models
{
    public sealed class User
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }

        public string PhoneCode { get; set; }
        
        public string Phone { get; set; }

        public string ImageId { get; set; }

        public string Password { get; set; }

        public int VerificationCode { get; set; }
    }
}