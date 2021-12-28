using System;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Vpiska.Kafka.HostedServices;
using Vpiska.Kafka.Settings;

namespace Vpiska.Kafka
{
    public static class Entry
    {
        public static void AddKafkaConsumer<TConsumer, TEvent>(this IServiceCollection services,
            Action<KafkaConsumerSettings<TEvent>> settingsCallback)
            where TConsumer : class, IConsumer<TEvent>
        {
            var settings = new KafkaConsumerSettings<TEvent>();
            settingsCallback.Invoke(settings);
            services.AddSingleton(settings);
            services.AddTransient<IConsumer<TEvent>, TConsumer>();
            services.AddHostedService<KafkaConsumerHostedService<TEvent>>();
        }

        public static void AddKafkaProducer<TEvent>(this IServiceCollection services,
            Action<KafkaProducerSettings<TEvent>> settingsCallback)
        {
            var settings = new KafkaProducerSettings<TEvent>();
            settingsCallback.Invoke(settings);
            services.AddSingleton(settings);
            services.AddSingleton(Channel.CreateUnbounded<TEvent>());
            services.AddHostedService<KafkaProducerHostedService<TEvent>>();
            services.AddSingleton<IProducer<TEvent>, KafkaProducer<TEvent>>();
        }
    }
}