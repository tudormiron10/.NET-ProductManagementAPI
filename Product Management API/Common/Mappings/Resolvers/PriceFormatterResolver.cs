using AutoMapper;
using Product_Management_API.Features;
using Product_Management_API.Features.Products;

namespace Product_Management_API.Mapping.Resolvers
{
    public class PriceFormatterResolver : IValueResolver<Product, ProductProfileDTO, string>
    {
        public string Resolve(Product source, ProductProfileDTO destination, string destMember, ResolutionContext context)
        {
            // Notă: Logica de discount din Task 1.2 se aplică pe valoarea decimală (Price).
            // Aici formatăm valoarea "brută" sau valoarea deja modificată dacă resolverul este apelat după maparea prețului.
            // Documentul cere simplu: Format as currency using ToString("C2") [cite: 79]
            
            return source.Price.ToString("C2");
        }
    }
}