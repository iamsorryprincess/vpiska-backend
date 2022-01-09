namespace Vpiska.Api.Constants
{
    public static class EventConstants
    {
        #region ValidationErrors

        public const string IdIsEmpty = "IdIsEmpty";
        public const string InvalidIdFormat = "InvalidIdFormat";
        public const string NameIsEmpty = "NameIsEmpty";
        public const string AddressIsEmpty = "AddressIsEmpty";
        public const string CoordinatesAreEmpty = "CoordinatesAreEmpty";
        public const string HorizontalRangeIsEmpty = "HorizontalRangeIsEmpty";
        public const string VerticalRangeIsEmpty = "VerticalRangeIsEmpty";

        #endregion

        #region DomainErrors

        public const string OwnerAlreadyHasEvent = "OwnerAlreadyHasEvent";
        public const string EventNotFound = "EventNotFound";

        #endregion
    }
}