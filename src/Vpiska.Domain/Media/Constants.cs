namespace Vpiska.Domain.Media
{
    public static class Constants
    {
        public const string Path = "media";

        #region ValidationErrors

        public const string NameIsEmpty = "NameIsEmpty";

        #endregion
        
        #region DomainErrors
        
        public const string ContentTypeNotSupported = "ContentTypeNotSupported";
        public const string MediaNotFound = "MediaNotFound";

        #endregion
    }
}