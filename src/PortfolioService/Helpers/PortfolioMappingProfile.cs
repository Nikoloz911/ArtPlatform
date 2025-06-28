using AutoMapper;
using PortfolioService.DTOs;
using PortfolioService.Models;

namespace PortfolioService.Helpers;
public class PortfolioMappingProfile : Profile
{
    public PortfolioMappingProfile()
    {
        CreateMap<AddPortfolioDTO, Portfolio>()
            .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(_ => DateOnly.FromDateTime(DateTime.UtcNow)));

        CreateMap<Portfolio, AddPortfolioResponseDTO>();

        CreateMap<UpdatePortfolioDTO, Portfolio>();
        CreateMap<Portfolio, UpdatePortfolioResponseDTO>();
    }
}