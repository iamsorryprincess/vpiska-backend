using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vpiska.Domain.EventAggregate.Repository;
using Vpiska.Orleans.Repository;

namespace Vpiska.Orleans
{
    public static class Entry
    {
        public static void AddOrleans(this IServiceCollection services, IConfigurationSection orleansSection)
        {
            var areaOptions = new AreaSettings();
            orleansSection.Bind(areaOptions);
            services.AddSingleton(areaOptions);
            services.AddTransient<ICheckAreaRepository, CheckAreaRepository>();
            services.AddTransient<ICheckEventRepository, CheckEventRepository>();
            services.AddTransient<ICloseEventRepository, CloseEventRepository>();
            services.AddTransient<ICreateEventRepository, CreateEventRepository>();
            services.AddTransient<IGetByIdRepository, GetByIdRepository>();
            services.AddTransient<IGetEventsRepository, GetEventsRepository>();
            services.AddTransient<IAddMediaRepository, AddMediaRepository>();
            services.AddTransient<IRemoveMediaRepository, RemoveMediaRepository>();
            services.AddTransient<IChatMessageRepository, ChatMessageRepository>();
        }
    }
}