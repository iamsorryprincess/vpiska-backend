namespace Vpiska.Api.Constants
{
    public static class UserConstants
    {
        public const string PhoneRegex = @"^\d{10}\b$";
        public const string PhoneCode = "+7";
        public const int PasswordLength = 6;
        public const int MinCodeLength = 100000;
        public const int MaxCodeLength = 999999;

        #region ValidationErrors

        public const string IdIsEmpty = "IdIsEmpty";
        public const string NameIsEmpty = "NameIsEmpty";
        public const string PhoneIsEmpty = "PhoneIsEmpty";
        public const string PhoneRegexInvalid = "PhoneRegexInvalid";
        public const string PasswordIsEmpty = "PasswordIsEmpty";
        public const string PasswordLengthInvalid = "PasswordLengthInvalid";
        public const string ConfirmPasswordInvalid = "ConfirmPasswordInvalid";
        public const string CodeIsEmpty = "CodeIsEmpty";
        public const string CodeLengthInvalid = "CodeLengthInvalid";
        public const string TokenIsEmpty = "TokenIsEmpty";
        public const string IdInvalidFormat = "IdInvalidFormat";

        #endregion

        #region DomainErrors

        public const string PhoneAlreadyUse = "PhoneAlreadyUse";
        public const string NameAlreadyUse = "NameAlreadyUse";
        public const string UserNotFound = "UserNotFound";
        public const string UserByPhoneNotFound = "UserByPhoneNotFound";
        public const string InvalidPassword = "InvalidPassword";
        public const string InvalidCode = "InvalidCode";

        #endregion
    }
}