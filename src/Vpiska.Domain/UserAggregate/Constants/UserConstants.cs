namespace Vpiska.Domain.UserAggregate.Constants
{
    public static class UserConstants
    {
        public const string PhoneRegex = @"^\d{10}\b$";
        public const string PhoneCode = "+7";
        public const int PasswordLength = 6;
        public const int VerificationCodeMin = 100000;
        public const int VerificationCodeMax = 777777;
    }
}