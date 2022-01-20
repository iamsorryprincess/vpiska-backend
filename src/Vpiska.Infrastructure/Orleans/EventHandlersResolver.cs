using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Vpiska.Domain.Event.Events.ChatMessageEvent;
using Vpiska.Domain.Event.Events.EventClosedEvent;
using Vpiska.Domain.Event.Events.MediaAddedEvent;
using Vpiska.Domain.Event.Events.MediaRemovedEvent;
using Vpiska.Domain.Event.Events.UserConnectedEvent;
using Vpiska.Domain.Event.Events.UserDisconnectedEvent;
using Vpiska.Domain.Event.Interfaces;

namespace Vpiska.Infrastructure.Orleans
{
    internal sealed class EventHandlersResolver
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EventHandlersResolver(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task Resolve(IDomainEvent domainEvent)
        {
            using var scope = _scopeFactory.CreateAsyncScope();
            return domainEvent switch
            {
                UserConnectedEvent data => Handle(scope.ServiceProvider, data),
                UserDisconnectedEvent data => Handle(scope.ServiceProvider, data),
                ChatMessageEvent data => Handle(scope.ServiceProvider, data),
                EventClosedEvent data => Handle(scope.ServiceProvider, data),
                MediaAddedEvent data => Handle(scope.ServiceProvider, data),
                MediaRemovedEvent data => Handle(scope.ServiceProvider, data),
                _ => Task.CompletedTask
            };
        }

        private Task Handle<TEvent>(IServiceProvider serviceProvider, TEvent domainEvent) where TEvent : IDomainEvent
        {
            var handler = serviceProvider.GetService<IEventHandler<TEvent>>();
            return handler != null ? handler.Handle(domainEvent) : Task.CompletedTask;
        }
    }
}