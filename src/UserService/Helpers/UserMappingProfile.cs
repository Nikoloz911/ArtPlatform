using AutoMapper;
using UserService.DTOs;
using UserService.Models;
namespace UserService.Helpers;
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<UpdateUserDTO, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
