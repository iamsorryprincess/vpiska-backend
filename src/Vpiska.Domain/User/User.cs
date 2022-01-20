using System;

namespace Vpiska.Domain.User
{
    public sealed class User
    {
        public string Id { get; }

        public string Name { get; }

        public string PhoneCode { get; }

        public string Phone { get; }

        public string ImageId { get; }

        public string Password { get; }

        public int VerificationCode { get; }

        public User(string id, string name, string phoneCode, string phone, string imageId, string password, int verificationCode)
        {
            Id = id;
            Name = name;
            PhoneCode = phoneCode;
            Phone = phone;
            ImageId = imageId;
            Password = password;
            VerificationCode = verificationCode;
        }
    }
}