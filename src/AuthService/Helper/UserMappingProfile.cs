using AuthService.DTOs;
using AutoMapper;
using AuthService.Models;
using Contracts.DTO;

namespace AuthService.Helper;
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<RegisterUserDTO, User>();
        CreateMap<User, UserCreatedEvent>();
    }
}
