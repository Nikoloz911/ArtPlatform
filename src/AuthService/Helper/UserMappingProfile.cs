using AuthService.DTOs;
using AuthService.Models;
using AutoMapper;
using Contracts.Core;
using Contracts.DTO;

namespace AuthService.Helper;
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<RegisterUserDTO, User>();
        CreateMap<User, UserCreatedEvent>();
        CreateMap<User, JWTUserModel>();
        CreateMap<User, LoginResponseDTO>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
        CreateMap<User, UserVerifiedEvent>();
    }
}
