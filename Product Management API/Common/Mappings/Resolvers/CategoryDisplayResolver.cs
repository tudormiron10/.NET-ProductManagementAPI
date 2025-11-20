using AutoMapper;
using Product_Management_API.Features;
using Product_Management_API.Features.Products;

namespace Product_Management_API.Mapping.Resolvers
{
    public class CategoryDisplayResolver : IValueResolver<Product, ProductProfileDTO, string>
    {
        public string Resolve(Product source, ProductProfileDTO destination, string destMember, ResolutionContext context)
        {
            // [cite: 70-75] Mapare specifică pentru categorii
            return source.Category switch
            {
                ProductCategory.Electronics => "Electronics & Technology",
                ProductCategory.Clothing => "Clothing & Fashion",
                ProductCategory.Books => "Books & Media",
                ProductCategory.Home => "Home & Garden",
                _ => "Uncategorized" // [cite: 75] Default Case
            };
        }
    }
}