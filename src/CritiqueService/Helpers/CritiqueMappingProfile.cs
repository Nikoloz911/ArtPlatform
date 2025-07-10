using AutoMapper;
using CritiqueService.DTOs;
using CritiqueService.Models;

namespace CritiqueService.Helpers;
public class CritiqueMappingProfile : Profile
{
    public CritiqueMappingProfile()
    {
        CreateMap<AddCritiqueDTO, Critique>();
        CreateMap<Critique, AddCritiqueResponseDTO>();
        CreateMap<UpdateCritiqueDTO, Critique>()
              .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Critique, UpdateCritiqueResponseDTO>();
    }
}
