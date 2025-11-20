using Product_Management_API.Features.Products; 

namespace Product_Management_API.Common.Logging
{
    public record ProductCreationMetrics
    {
        public string OperationId { get; init; } = string.Empty;
        public string ProductName { get; init; } = string.Empty;
        public string SKU { get; init; } = string.Empty;
        public ProductCategory Category { get; init; }
        
        public TimeSpan ValidationDuration { get; init; }
        public TimeSpan DatabaseSaveDuration { get; init; }
        public TimeSpan TotalDuration { get; init; }
        
        public bool Success { get; init; }
        public string? ErrorReason { get; init; }
    }
}