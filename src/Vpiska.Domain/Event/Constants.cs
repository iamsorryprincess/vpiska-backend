namespace Vpiska.Domain.Event
{
    public static class Constants
    {
        #region ValidationErrors

        public const string IdIsEmpty = "IdIsEmpty";
        public const string InvalidIdFormat = "InvalidIdFormat";
        public const string NameIsEmpty = "NameIsEmpty";
        public const string AddressIsEmpty = "AddressIsEmpty";
        public const string CoordinatesAreEmpty = "CoordinatesAreEmpty";
        public const string HorizontalRangeIsEmpty = "HorizontalRangeIsEmpty";
        public const string VerticalRangeIsEmpty = "VerticalRangeIsEmpty";
        public const string MediaIsEmpty = "MediaIsEmpty";
        public const string MediaContentTypeIsEmpty = "MediaContentTypeIsEmpty";
        public const string MediaIdIsEmpty = "MediaIdIsEmpty";
        public const string MediaNotFound = "MediaNotFound";

        #endregion

        #region DomainErrors

        public const string OwnerAlreadyHasEvent = "OwnerAlreadyHasEvent";
        public const string EventNotFound = "EventNotFound";
        public const string UserIsNotOwner = "UserIsNotOwner";

        #endregion
    }
}