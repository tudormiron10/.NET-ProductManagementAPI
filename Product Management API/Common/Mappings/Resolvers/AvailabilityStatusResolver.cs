using AutoMapper;
using Product_Management_API.Features;
using Product_Management_API.Features.Products;

namespace Product_Management_API.Mapping.Resolvers
{
    public class AvailabilityStatusResolver : IValueResolver<Product, ProductProfileDTO, string>
    {
        public string Resolve(Product source, ProductProfileDTO destination, string destMember, ResolutionContext context)
        {
            // [cite: 92] Not available -> "Out of Stock" (Prioritate maximă)
            if (!source.IsAvailable) return "Out of Stock";

            // [cite: 93] Available, 0 stock -> "Unavailable"
            if (source.StockQuantity == 0) return "Unavailable";

            // [cite: 94] Available, 1 stock -> "Last Item"
            if (source.StockQuantity == 1) return "Last Item";

            // [cite: 95] Available, <= 5 stock -> "Limited Stock"
            if (source.StockQuantity <= 5) return "Limited Stock";

            // [cite: 96] Available, > 5 stock -> "In Stock"
            return "In Stock";
        }
    }
}