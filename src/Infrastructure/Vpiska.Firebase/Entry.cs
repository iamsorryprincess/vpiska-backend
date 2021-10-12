using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.DependencyInjection;
using Vpiska.Domain;

namespace Vpiska.Firebase
{
    public static class Entry
    {
        public static void AddFirebase(this IServiceCollection services)
        {
#if DEBUG
            const string path = "../Infrastructure/Vpiska.Firebase/firebase.json";
#else
            const string path = "firebase.json";
#endif

            services.AddSingleton(FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(path)
            }));

            services.AddSingleton(StorageClient.Create(GoogleCredential.FromFile(path)));
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<IFileStorage, FileStorage>();
        }
    }
}