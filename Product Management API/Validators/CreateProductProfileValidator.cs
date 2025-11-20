using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Product_Management_API.Data;
using Product_Management_API.Features.Products;
using System.Text.RegularExpressions;

namespace Product_Management_API.Validators
{
    public class CreateProductProfileValidator : AbstractValidator<CreateProductProfileRequest>
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<CreateProductProfileValidator> _logger;

        private readonly List<string> _inappropriateWords = new() { "spam", "banned", "offensive", "illegal" };
        private readonly List<string> _techKeywords = new() { "smart", "digital", "electric", "tech", "wireless", "auto", "phone", "screen" };
        private readonly List<string> _inappropriateHomeWords = new() { "toxic", "industrial", "hazardous", "medical" };

        public CreateProductProfileValidator(ApplicationContext context, ILogger<CreateProductProfileValidator> logger)
        {
            _context = context;
            _logger = logger;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .Length(1, 200).WithMessage("Name must be between 1 and 200 characters.")
                .Must(BeValidName).WithMessage("Name contains inappropriate content.")
                .MustAsync(BeUniqueName).WithMessage("Product name must be unique for this brand.");

            RuleFor(x => x.Brand)
                .NotEmpty().WithMessage("Brand is required.")
                .Length(2, 100).WithMessage("Brand must be between 2 and 100 characters.")
                .Must(BeValidBrandName).WithMessage("Brand contains invalid characters.");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required.")
                .Length(5, 20).WithMessage("SKU must be between 5 and 20 characters.")
                .Must(BeValidSKU).WithMessage("SKU must be alphanumeric with hyphens.")
                .MustAsync(BeUniqueSKU).WithMessage("SKU already exists in the system.");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid product category.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.")
                .LessThan(10000).WithMessage("Price must be less than $10,000.");

            RuleFor(x => x.ReleaseDate)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Release date cannot be in the future.")
                .GreaterThan(new DateTime(1900, 1, 1)).WithMessage("Release date cannot be before year 1900.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.")
                .LessThanOrEqualTo(100000).WithMessage("Stock cannot exceed 100,000.");

            RuleFor(x => x.ImageUrl)
                .Must(BeValidImageUrl)
                .When(x => !string.IsNullOrEmpty(x.ImageUrl)) // Doar dacă e completat
                .WithMessage("Invalid Image URL. Must be HTTP/HTTPS and end with a valid image extension.");

            RuleFor(x => x)
                .MustAsync(PassBusinessRules).WithMessage("One or more business rules failed. See logs for details.");
            
            RuleFor(x => x)
                .MustAsync(PassBusinessRules)
                .WithMessage("One or more complex business rules failed. Check logs.");
        }

        private bool BeValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return !_inappropriateWords.Any(badWord => name.ToLower().Contains(badWord));
        }

        private async Task<bool> BeUniqueName(CreateProductProfileRequest model, string name, CancellationToken token)
        {
            _logger.LogInformation("Validating uniqueness for Name '{Name}' and Brand '{Brand}'", name, model.Brand);
            
            var exists = await _context.Products
                .AnyAsync(p => p.Name == name && p.Brand == model.Brand, token);
            
            return !exists;
        }

        private bool BeValidBrandName(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand)) return false;
            // Letters, spaces, hyphens, apostrophes, dots, numbers
            var regex = new Regex(@"^[a-zA-Z0-9\s\-\'\.]+$");
            return regex.IsMatch(brand);
        }

        private bool BeValidSKU(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku)) return false;
            var regex = new Regex(@"^[a-zA-Z0-9\-]+$");
            return regex.IsMatch(sku);
        }

        private async Task<bool> BeUniqueSKU(string sku, CancellationToken token)
        {
            _logger.LogInformation("Validating uniqueness for SKU '{SKU}'", sku);
            var exists = await _context.Products.AnyAsync(p => p.SKU == sku, token);
            return !exists;
        }

        private bool BeValidImageUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return true; // Optional

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                return false;
            }

            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            return validExtensions.Any(ext => url.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }
        
        private bool ContainTechnologyKeywords(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var lowerName = name.ToLower();
            return _techKeywords.Any(keyword => lowerName.Contains(keyword));
        }
        
        private bool BeAppropriateForHome(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var lowerName = name.ToLower();
            // Returnează false dacă GĂSEȘTE un cuvânt nepotrivit
            return !_inappropriateHomeWords.Any(badWord => lowerName.Contains(badWord));
        }

        private async Task<bool> PassBusinessRules(CreateProductProfileRequest model, CancellationToken token)
        {
            _logger.LogInformation("Evaluating Business Rules for {Name} (Category: {Category})", model.Name, model.Category);

            // 1. Daily Limit Check (Task 3.1) - Rule 1
            var todayCount = await _context.Products.CountAsync(p => p.CreatedAt >= DateTime.UtcNow.Date, token);
            if (todayCount >= 500)
            {
                _logger.LogWarning("Rule Failed: Daily limit (500) reached.");
                return false;
            }

            if (model.Category == ProductCategory.Electronics)
            {
                // Price minimum $50.00
                if (model.Price < 50) 
                {
                    _logger.LogWarning("Rule Failed: Electronics must be at least $50.");
                    return false;
                }

                // Must contain technology keywords in Name
                if (!ContainTechnologyKeywords(model.Name))
                {
                    _logger.LogWarning("Rule Failed: Electronics name must contain tech keywords.");
                    return false;
                }

                // Must be released within last 5 years
                var fiveYearsAgo = DateTime.UtcNow.AddYears(-5);
                if (model.ReleaseDate < fiveYearsAgo)
                {
                    _logger.LogWarning("Rule Failed: Electronics must be released within last 5 years.");
                    return false;
                }
            }

            if (model.Category == ProductCategory.Home)
            {
                // Price maximum $200.00
                if (model.Price > 200)
                {
                    _logger.LogWarning("Rule Failed: Home products cannot exceed $200.");
                    return false;
                }

                // Name must be appropriate (helper check)
                if (!BeAppropriateForHome(model.Name))
                {
                    _logger.LogWarning("Rule Failed: Home product name contains restricted words.");
                    return false;
                }
            }

            if (model.Category == ProductCategory.Clothing)
            {
                // Brand name minimum 3 characters
                if (model.Brand.Length < 3)
                {
                    _logger.LogWarning("Rule Failed: Clothing brand must be at least 3 chars.");
                    return false;
                }
            }

            if (model.Price > 100 && model.StockQuantity > 20)
            {
                _logger.LogWarning("Rule Failed: Expensive products (>$100) cannot have stock > 20.");
                return false;
            }

            return true;
        }
    }
}