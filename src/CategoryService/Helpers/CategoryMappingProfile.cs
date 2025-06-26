using AutoMapper;
using CategoryService.Models;
using CategoryService.DTOs;

namespace CategoryService.Helpers;
public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile() {
        CreateMap<Category, GetCategoriesDTO>();
        CreateMap<Category, GetCategoriesByIdDTO>();
        CreateMap<AddCategoryDTO, Category>();
        CreateMap<Category, AddCategoryResponseDTO>();
        CreateMap<UpdateCategoryDTO, Category>();
        CreateMap<Category, UpdateCategoryResponseDTO>();
    }
}
