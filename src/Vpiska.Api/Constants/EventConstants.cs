namespace Vpiska.Api.Constants
{
    public static class EventConstants
    {
        #region ValidationErrors

        public const string IdIsEmpty = "IdIsEmpty";
        public const string NameIsEmpty = "NameIsEmpty";
        public const string AddressIsEmpty = "AddressIsEmpty";
        public const string CoordinatesIsEmpty = "CoordinatesIsEmpty";
        public const string RangeIsEmpty = "RangeIsEmpty";
        public const string MediaIsEmpty = "MediaIsEmpty";
        public const string MediaIdIsEmpty = "MediaIdIsEmpty";

        #endregion

        #region DomainErrors

        public const string OwnerAlreadyHasEvent = "OwnerAlreadyHasEvent";
        public const string EventNotFound = "EventNotFound";
        public const string UserIsNotOwner = "UserIsNotOwner";
        public const string MediaNotFound = "MediaNotFound";

        #endregion
    }
}