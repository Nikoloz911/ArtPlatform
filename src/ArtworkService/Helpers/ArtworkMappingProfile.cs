using ArtworkService.DTOs;
using ArtworkService.Models;
using AutoMapper;

namespace ArtworkService.Helpers;
public class ArtworkMappingProfile : Profile
{
    public ArtworkMappingProfile()
    {
        CreateMap<AddArtworkDTO, Artwork>();
        CreateMap<Artwork, ArtworkResponseDTO>();
        CreateMap<Artwork, UpdateArtworkResponseDTO>();
        CreateMap<UpdateArtworkDTO, Artwork>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
