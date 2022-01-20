namespace Vpiska.Domain.User.Queries.CheckCodeQuery
{
    public sealed class CheckCodeQuery
    {
        public string Phone { get; set; }

        public int? Code { get; set; }
    }
}