using AutoMapper;
using Product_Management_API.Features;
using Product_Management_API.Features.Products;

namespace Product_Management_API.Mapping.Resolvers
{
    public class BrandInitialsResolver : IValueResolver<Product, ProductProfileDTO, string>
    {
        public string Resolve(Product source, ProductProfileDTO destination, string destMember, ResolutionContext context)
        {
            // [cite: 89] No brand -> "?"
            if (string.IsNullOrWhiteSpace(source.Brand)) return "?";

            var parts = source.Brand.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                // [cite: 87] Two+ words -> First letter of first and last words (uppercase)
                return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[^1][0])}";
            }
            
            if (parts.Length == 1)
            {
                // [cite: 88] Single word -> First letter (uppercase)
                return char.ToUpper(parts[0][0]).ToString();
            }

            return "?";
        }
    }
}