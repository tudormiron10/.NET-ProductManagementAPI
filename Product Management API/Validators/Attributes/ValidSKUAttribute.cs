using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Product_Management_API.Validators.Attributes
{
    public class ValidSKUAttribute : ValidationAttribute, IClientModelValidator
    {
        public ValidSKUAttribute()
        {
            ErrorMessage = "SKU must be 5-20 alphanumeric characters with hyphens.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string sku)
            {
                return ValidationResult.Success; 
            }

            var cleanSku = sku.Replace(" ", "");

            if (cleanSku.Length < 5 || cleanSku.Length > 20)
            {
                return new ValidationResult("SKU length must be between 5 and 20 characters.");
            }

            var regex = new Regex("^[a-zA-Z0-9\\-]+$");
            if (!regex.IsMatch(cleanSku))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.Attributes.ContainsKey("data-val"))
            {
                context.Attributes.Add("data-val", "true");
            }

            context.Attributes["data-val-regex"] = ErrorMessage ?? "Invalid SKU format";
            context.Attributes["data-val-regex-pattern"] = "^[a-zA-Z0-9\\-]+$";
        }
    }
}