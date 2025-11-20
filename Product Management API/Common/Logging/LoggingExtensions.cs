using Microsoft.Extensions.Logging;
using System;

namespace Product_Management_API.Common.Logging
{
    public static class LoggingExtensions
    {
        public static void LogProductCreationMetrics(this ILogger logger, ProductCreationMetrics metrics)
        {
            logger.LogInformation(
            new EventId(LogEvents.ProductCreationCompleted, nameof(LogEvents.ProductCreationCompleted)),
            
            "Product Creation Metrics for {Name} ({SKU}) - Category: {Category} | Success: {Success} | Total: {TotalDuration}ms | Validation: {ValidationDuration}ms | DB: {DatabaseDuration}ms | Error: {ErrorReason}",
                
            metrics.ProductName,                            // {Name}
            metrics.SKU,                                    // {SKU}
            metrics.Category.ToString(),                    // {Category}
            metrics.Success,                                // {Success}
            metrics.TotalDuration.TotalMilliseconds,        // {TotalDuration}
            metrics.ValidationDuration.TotalMilliseconds,   // {ValidationDuration}
            metrics.DatabaseSaveDuration.TotalMilliseconds, // {DatabaseDuration}
            metrics.ErrorReason                             // {ErrorReason}
                );
        }
    }
}