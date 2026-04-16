namespace MODELS.ProcureAccess.Entities.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false));
        CreateMap<Criterion, CriterionDto>();
        CreateMap<Criterion, CriterionDto>().ReverseMap();
        CreateMap<CreateCriterionDto, Criterion>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false));
        CreateMap<CriteriaFilter, CriteriaFilterDto>();
        CreateMap<CriteriaFilter, CriteriaFilterDto>().ReverseMap();
        CreateMap<FilterType, FilterTypeDto>();
        CreateMap<FilterType, FilterTypeDto>().ReverseMap();
        CreateMap<ProductPart, ProductPartDto>();
        CreateMap<ProductPart, ProductPartDto>().ReverseMap();
        CreateMap<ProductTest, ProductTestDto>();
        CreateMap<ProductTest, ProductTestDto>().ReverseMap();
        CreateMap<ProductType, ProductTypeDto>();
        CreateMap<ProductType, ProductTypeDto>().ReverseMap();
        CreateMap<Proposal, ProposalDto>();
        // CreateMap<Proposal, ProposalDto>().ReverseMap();

        CreateMap<User, UserDto>();
        CreateMap<UICustomization, UICustomizationDto>();
    }
}
