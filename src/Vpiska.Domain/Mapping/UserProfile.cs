using System;
using AutoMapper;
using Vpiska.Domain.Models;
using Vpiska.Domain.Requests;

namespace Vpiska.Domain.Mapping
{
    public sealed class UserProfile : Profile
    {
        private const string PhoneCode = "+7";
        
        public UserProfile()
        {
            CreateMap<CreateUserRequest, UserModel>()
                .ForMember(x => x.Id,
                    x => x.MapFrom(_ => Guid.NewGuid().ToString("N")))
                .ForMember(x => x.PhoneCode,
                    x => x.MapFrom(_ => PhoneCode));
        }
    }
}