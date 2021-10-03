namespace Vpiska.Domain.Models
{
    public sealed class NamePhoneCheckResult
    {
        public bool IsNameExist { get; set; }
        
        public bool IsPhoneExist { get; set; }
    }
}