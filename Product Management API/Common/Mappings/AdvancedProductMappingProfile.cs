using Product_Management_API.Features.Products;
using AutoMapper;
using Product_Management_API.Mapping.Resolvers;

namespace Product_Management_API.Mappings;

public class AdvancedProductMappingProfile : Profile
{
    public AdvancedProductMappingProfile()
    {
        CreateMap<CreateProductProfileRequest, Product>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.StockQuantity > 0))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<Product, ProductProfileDTO>()
            .ForMember(dest => dest.CategoryDisplayName, opt => opt.MapFrom<CategoryDisplayResolver>())
            .ForMember(dest => dest.ProductAge, opt => opt.MapFrom<ProductAgeResolver>())
            .ForMember(dest => dest.BrandInitials, opt => opt.MapFrom<BrandInitialsResolver>())
            .ForMember(dest => dest.FormattedPrice, opt => opt.MapFrom<PriceFormatterResolver>())
            .ForMember(dest => dest.AvailabilityStatus, opt => opt.MapFrom<AvailabilityStatusResolver>())
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src =>
                src.Category == ProductCategory.Home
                    ? src.Price * 0.9m
                    : src.Price))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                src.Category == ProductCategory.Home
                    ? null
                    : src.ImageUrl));
    }   
}
