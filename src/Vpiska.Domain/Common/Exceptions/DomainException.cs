using System;

namespace Vpiska.Domain.Common.Exceptions
{
    [Serializable]
    public abstract class DomainException : Exception
    {
        public string[] ErrorsCodes { get; }

        protected DomainException(params string[] errorsCodes) : base("domain exception")
        {
            ErrorsCodes = errorsCodes;
        }
    }
}