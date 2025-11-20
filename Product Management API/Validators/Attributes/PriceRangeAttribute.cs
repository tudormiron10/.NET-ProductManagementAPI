using System.ComponentModel.DataAnnotations;

namespace Product_Management_API.Validators.Attributes
{
    public class PriceRangeAttribute : ValidationAttribute
    {
        private readonly decimal _minPrice;
        private readonly decimal _maxPrice;

        public PriceRangeAttribute(double min, double max)
        {
            _minPrice = (decimal)min;
            _maxPrice = (decimal)max;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not decimal price)
            {
                return ValidationResult.Success;
            }

            if (price < _minPrice || price > _maxPrice)
            {
                return new ValidationResult($"Price must be between {_minPrice:C2} and {_maxPrice:C2}.");
            }

            return ValidationResult.Success;
        }
    }
}