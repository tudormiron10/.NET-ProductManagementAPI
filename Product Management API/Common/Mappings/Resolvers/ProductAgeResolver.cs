using AutoMapper;
using Product_Management_API.Features;
using Product_Management_API.Features.Products;

namespace Product_Management_API.Mapping.Resolvers
{
    public class ProductAgeResolver : IValueResolver<Product, ProductProfileDTO, string>
    {
        public string Resolve(Product source, ProductProfileDTO destination, string destMember, ResolutionContext context)
        {
            var days = (DateTime.UtcNow - source.ReleaseDate).TotalDays;

            // [cite: 82] < 30 days
            if (days < 30) return "New Release";

            // [cite: 83] < 365 days
            if (days < 365) return $"{Math.Floor(days / 30)} months old";

            // [cite: 84] < 1825 days (5 years)
            if (days < 1825) return $"{Math.Floor(days / 365)} years old";

            // [cite: 85] >= 1825 days
            return "Classic";
        }
    }
}