using System;
using AutoMapper;
using Vpiska.Api.Auth;
using Vpiska.Api.Dto;
using Vpiska.Mongo.Models;

namespace Vpiska.Api.Mapping
{
    public sealed class UserProfile : Profile
    {
        private const string PhoneCode = "+7";
        
        public UserProfile()
        {
            CreateMap<CreateUserRequest, User>()
                .ForMember(x => x.Id,
                    x => x.MapFrom(_ => Guid.NewGuid().ToString("N")))
                .ForMember(x => x.PhoneCode,
                    x => x.MapFrom(_ => PhoneCode))
                .ForMember(x => x.Password,
                    x => x.MapFrom(a => a.Password.HashPassword()));
        }
    }
}