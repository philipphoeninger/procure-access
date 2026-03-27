namespace MODELS.ProcureAccess.Entities.Dto;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<Criterion, CriterionDto>().ReverseMap();
        CreateMap<CriteriaFilter, CriteriaFilterDto>().ReverseMap();
        CreateMap<FilterType, FilterTypeDto>().ReverseMap();
        CreateMap<ProductPart, ProductPartDto>().ReverseMap();
        CreateMap<ProductTest, ProductTestDto>().ReverseMap();
    }
}
