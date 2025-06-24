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
    }
}
