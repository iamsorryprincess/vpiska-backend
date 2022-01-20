using System;

namespace Vpiska.Domain.Common.Exceptions
{
    [Serializable]
    public sealed class ValidationException : Exception
    {
        public string[] ErrorsCodes { get; }

        public ValidationException(string[] errorCodes) : base("Invalid request")
        {
            ErrorsCodes = errorCodes;
        }
    }
}