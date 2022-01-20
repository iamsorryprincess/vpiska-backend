namespace Vpiska.Infrastructure.Firebase
{
    internal sealed class FirebaseSettings
    {
        public string BucketName { get; }

        public FirebaseSettings(string bucketName)
        {
            BucketName = bucketName;
        }
    }
}