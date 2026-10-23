// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
		CreateMap<Product, ProductDto>()
    		.ForMember(
        		dest => dest.CriteriaFilters,
        		opt => opt.MapFrom(
            		src => src.ProductCriteriaFilters
                		.Select(x => x.CriteriaFilter)));

        // Create DTOs carry only client input. Members set by the server or by EF (key, audit
        // fields, concurrency token, navigations) are ignored explicitly, so that
        // AssertConfigurationIsValid (ARC-06) flags any other destination member left unmapped.
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.ProductCriteriaFilters,
                opt => opt.MapFrom(src =>
                    src.CriteriaFilterIds.Select(id =>
                        new ProductCriteriaFilter
                        {
                            CriteriaFilterId = id
                        })))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.xmin, opt => opt.Ignore())
            .ForMember(dest => dest.Proposal, opt => opt.Ignore());

        CreateMap<Criterion, CriterionDto>();
        CreateMap<CreateCriterionDto, Criterion>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.xmin, opt => opt.Ignore())
            .ForMember(dest => dest.CriteriaFilter, opt => opt.Ignore())
            .ForMember(dest => dest.Proposal, opt => opt.Ignore());

        CreateMap<CriteriaFilter, LookupDto>();
		CreateMap<CriteriaFilter, CriteriaFilterDto>()
    		.ForMember(
        		dest => dest.Exclusions,
        		opt => opt.MapFrom(
            		src => src.Exclusions
                		.Select(x => x.Exclusion)));

        CreateMap<FilterType, FilterTypeDto>();
        CreateMap<Proposal, ProposalDto>();

        CreateMap<User, UserDto>();
        CreateMap<UICustomization, UICustomizationDto>()
            .ForMember(dest => dest.Language,
                opt => opt.ConvertUsing(new EnumToLanguageConverter(), src => src.Language))
            .ReverseMap()
            .ForMember(dest => dest.Language,
                opt => opt.ConvertUsing(new LanguageToEnumConverter(), src => src.Language));
    }
}
