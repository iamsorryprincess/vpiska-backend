namespace Vpiska.Domain.Validation
{
    public static class ErrorCodes
    {
        // common
        public const string InternalError = "internalError";
        
        // create user
        public const string PhoneIsEmpty = "phoneIsEmpty";
        public const string PhoneRegexNotPassed = "phoneRegexNotPassed";
        public const string PhoneIsAlreadyUse = "phoneIsAlreadyUse";
        public const string NameIsEmpty = "nameIsEmpty";
        public const string NameIsAlreadyUse = "nameIsAlreadyUse";
        public const string PasswordIsEmpty = "passwordIsEmpty";
        public const string PasswordLengthInvalid = "passwordLengthInvalid";
        public const string ConfirmPasswordInvalid = "confirmPasswordInvalid";
        
        // login user
        public const string UserByPhoneNotFound = "userByPhoneNotFound";
        public const string InvalidPassword = "invalidPassword";
    }
}