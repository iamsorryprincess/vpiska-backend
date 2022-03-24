using System.Collections.Generic;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Vpiska.Domain.Common;
using Vpiska.Domain.Event.Commands.AddMediaCommand;
using Vpiska.Domain.Event.Commands.AddUserCommand;
using Vpiska.Domain.Event.Commands.ChangeLocationCommand;
using Vpiska.Domain.Event.Commands.ChangeUserPositionCommand;
using Vpiska.Domain.Event.Commands.CloseEventCommand;
using Vpiska.Domain.Event.Commands.CreateEventCommand;
using Vpiska.Domain.Event.Commands.RemoveMediaCommand;
using Vpiska.Domain.Event.Commands.RemoveUserCommand;
using Vpiska.Domain.Event.Events.ChatMessageEvent;
using Vpiska.Domain.Event.Events.EventClosedEvent;
using Vpiska.Domain.Event.Events.EventCreatedEvent;
using Vpiska.Domain.Event.Events.EventUpdatedEvent;
using Vpiska.Domain.Event.Events.MediaAddedEvent;
using Vpiska.Domain.Event.Events.MediaRemovedEvent;
using Vpiska.Domain.Event.Events.UserConnectedEvent;
using Vpiska.Domain.Event.Events.UserDisconnectedEvent;
using Vpiska.Domain.Event.Interfaces;
using Vpiska.Domain.Event.Queries.GetByIdQuery;
using Vpiska.Domain.Event.Queries.GetEventsQuery;
using Vpiska.Domain.Event.Responses;
using Vpiska.Domain.Interfaces;
using Vpiska.Domain.Media.Commands;
using Vpiska.Domain.Media.Commands.UploadMediaCommand;
using Vpiska.Domain.Media.Models;
using Vpiska.Domain.Media.Queries.GetByNameQuery;
using Vpiska.Domain.Media.Queries.PageQuery;
using Vpiska.Domain.User.Commands.ChangePasswordCommand;
using Vpiska.Domain.User.Commands.CreateUserCommand;
using Vpiska.Domain.User.Commands.LoginUserCommand;
using Vpiska.Domain.User.Commands.SetCodeCommand;
using Vpiska.Domain.User.Commands.UpdateUserCommand;
using Vpiska.Domain.User.Queries.CheckCodeQuery;
using Vpiska.Domain.User.Responses;

namespace Vpiska.Domain
{
    public static class Entry
    {
        public static void AddUserDomain(this IServiceCollection services)
        {
            services.AddTransient<IValidator<CreateUserCommand>, CreateUserValidator>();
            services.AddTransient<ICommandHandler<CreateUserCommand, LoginResponse>, CreateUserHandler>();
            
            services.AddTransient<IValidator<LoginUserCommand>, LoginUserValidator>();
            services.AddTransient<ICommandHandler<LoginUserCommand, LoginResponse>, LoginUserHandler>();

            services.AddTransient<IValidator<SetCodeCommand>, SetCodeValidator>();
            services.AddTransient<ICommandHandler<SetCodeCommand>, SetCodeHandler>();

            services.AddTransient<IValidator<CheckCodeQuery>, CheckCodeValidator>();
            services.AddTransient<IQueryHandler<CheckCodeQuery, LoginResponse>, CheckCodeHandler>();
            
            services.AddTransient<IValidator<ChangePasswordCommand>, ChangePasswordValidator>();
            services.AddTransient<ICommandHandler<ChangePasswordCommand>, ChangePasswordHandler>();

            services.AddTransient<IValidator<UpdateUserCommand>, UpdateUserValidator>();
            services.AddTransient<ICommandHandler<UpdateUserCommand, ImageIdResponse>, UpdateUserHandler>();
        }

        public static void AddEventDomain(this IServiceCollection services)
        {
            services.AddTransient<IValidator<CreateEventCommand>, CreateEventValidator>();
            services.AddTransient<ICommandHandler<CreateEventCommand, EventResponse>, CreateEventHandler>();
            services.AddTransient<IEventHandler<EventCreatedEvent>, EventCreatedHandler>();
            
            services.AddTransient<IValidator<GetEventsQuery>, GetEventsValidator>();
            services.AddTransient<IQueryHandler<GetEventsQuery, List<EventShortResponse>>, GetEventsHandler>();

            services.AddTransient<IValidator<GetByIdQuery>, GetByIdValidator>();
            services.AddTransient<IQueryHandler<GetByIdQuery, EventResponse>, GetByIdHandler>();

            services.AddTransient<IValidator<CloseEventCommand>, CloseEventValidator>();
            services.AddTransient<ICommandHandler<CloseEventCommand>, CloseEventHandler>();
            services.AddTransient<IEventHandler<EventClosedEvent>, EventClosedHandler>();

            services.AddTransient<IEventHandler<ChatMessageEvent>, ChatMessageHandler>();

            services.AddTransient<ICommandHandler<AddUserCommand>, AddUserHandler>();
            services.AddTransient<IEventHandler<UserConnectedEvent>, UserConnectedHandler>();

            services.AddTransient<ICommandHandler<RemoveUserCommand>, RemoveUserHandler>();
            services.AddTransient<IEventHandler<UserDisconnectedEvent>, UserDisconnectedHandler>();

            services.AddTransient<IValidator<AddMediaCommand>, AddMediaValidator>();
            services.AddTransient<ICommandHandler<AddMediaCommand>, AddMediaHandler>();
            services.AddTransient<IEventHandler<MediaAddedEvent>, MediaAddedHandler>();
            
            services.AddTransient<IValidator<RemoveMediaCommand>, RemoveMediaValidator>();
            services.AddTransient<ICommandHandler<RemoveMediaCommand>, RemoveMediaHandler>();
            services.AddTransient<IEventHandler<MediaRemovedEvent>, MediaRemovedHandler>();

            services.AddTransient<ICommandHandler<ChangeUserPositionCommand>, ChangeUserPositionHandler>();

            services.AddTransient<IValidator<ChangeLocationCommand>, ChangeLocationValidator>();
            services.AddTransient<ICommandHandler<ChangeLocationCommand>, ChangeLocationHandler>();
            services.AddTransient<IEventHandler<EventUpdatedEvent>, EventUpdatedHandler>();
        }

        public static void AddMediaDomain(this IServiceCollection services)
        {
            services.AddTransient<ICommandHandler<UploadMediaCommand, MetadataViewModel>, UploadMediaHandler>();
            services
                .AddTransient<ICommandHandler<Media.Commands.RemoveMediaCommand.RemoveMediaCommand>,
                    Media.Commands.RemoveMediaCommand.RemoveMediaHandler>();
            services.AddTransient<IQueryHandler<GetByNameQuery, Media.Media>, GetByNameHandler>();
            services.AddTransient<IQueryHandler<PageQuery, PagedResponse>, PageQueryHandler>();
        }
    }
}