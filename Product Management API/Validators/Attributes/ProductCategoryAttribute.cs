using Product_Management_API.Features.Products;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Product_Management_API.Validators.Attributes
{
    public class ProductCategoryAttribute : ValidationAttribute
    {
        private readonly ProductCategory[] _allowedCategories;

        public ProductCategoryAttribute(params ProductCategory[] allowedCategories)
        {
            _allowedCategories = allowedCategories;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not ProductCategory category)
            {
                return ValidationResult.Success;
            }

            if (!_allowedCategories.Contains(category))
            {
                var allowedList = string.Join(", ", _allowedCategories);
                return new ValidationResult($"Category not allowed. Allowed values: {allowedList}");
            }

            return ValidationResult.Success;
        }
    }
}