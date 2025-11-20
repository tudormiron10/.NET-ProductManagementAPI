using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using Product_Management_API.Common.Logging;
using Product_Management_API.Data;             

namespace Product_Management_API.Features.Products
{
    public class CreateProductHandler : IRequestHandler<CreateProductProfileRequest, ProductProfileDTO>
    {
        private readonly ApplicationContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateProductHandler> _logger;
        private readonly IMemoryCache _cache;

        public CreateProductHandler(
            ApplicationContext context,
            IMapper mapper,
            ILogger<CreateProductHandler> logger,
            IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<ProductProfileDTO> Handle(CreateProductProfileRequest request, CancellationToken cancellationToken)
        {
            // [Task 2.2] Generate unique operation ID
            var operationId = Guid.NewGuid().ToString("N")[..8];
            // var startTime = DateTime.UtcNow; // (Optional, folosim Stopwatch pentru precizie)
            var totalStopwatch = Stopwatch.StartNew();

            // Timers specifici pentru metrici
            var validationStopwatch = new Stopwatch();
            var dbStopwatch = new Stopwatch();

            // [Task 2.2] Use logging scope for entire product operation
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["OperationId"] = operationId,
                ["ProductName"] = request.Name,
                ["ProductBrand"] = request.Brand,
                ["ProductSKU"] = request.SKU,
                ["ProductCategory"] = request.Category
            }))
            {
                _logger.LogInformation(LogEvents.ProductCreationStarted,
                    "Starting creation for product {Name} ({SKU})", request.Name, request.SKU);

                try
                {
                    // --- VALIDATION PHASE ---
                    validationStopwatch.Start();

                    var skuExists = await _context.Products
                        .AnyAsync(p => p.SKU == request.SKU, cancellationToken);

                    if (skuExists)
                    {
                        _logger.LogWarning(LogEvents.ProductValidationFailed,
                            "Validation failed: SKU {SKU} already exists.", request.SKU);

                        throw new InvalidOperationException($"Product with SKU {request.SKU} already exists.");
                    }

                    _logger.LogDebug(LogEvents.SKUValidationPerformed,
                        "SKU Uniqueness check passed for {SKU}", request.SKU);

                    if (request.StockQuantity < 0)
                    {
                        throw new InvalidOperationException("Stock cannot be negative.");
                    }
                    _logger.LogDebug(LogEvents.StockValidationPerformed,
                        "Stock validation passed: {Stock}", request.StockQuantity);

                    validationStopwatch.Stop();
                    // Am sters backslash-ul de aici

                    // --- MAPPING PHASE ---
                    var productEntity = _mapper.Map<Product>(request);
                    // Am sters backslash-ul de aici

                    // --- DATABASE PHASE ---
                    dbStopwatch.Start();

                    _logger.LogInformation(LogEvents.DatabaseOperationStarted, "Saving product to database...");

                    _context.Products.Add(productEntity);
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(LogEvents.DatabaseOperationCompleted,
                        "Product saved successfully with ID: {ProductId}", productEntity.Id);

                    dbStopwatch.Stop();

                    // --- CACHING PHASE ---
                    _cache.Remove("all_products");
                    _logger.LogInformation(LogEvents.CacheOperationPerformed, "Cache invalidated for key: all_products");

                    totalStopwatch.Stop();

                    // --- RESPONSE & METRICS ---
                    var responseDto = _mapper.Map<ProductProfileDTO>(productEntity);

                    var metrics = new ProductCreationMetrics
                    {
                        OperationId = operationId,
                        ProductName = request.Name,
                        SKU = request.SKU,
                        Category = request.Category,
                        ValidationDuration = validationStopwatch.Elapsed,
                        DatabaseSaveDuration = dbStopwatch.Elapsed,
                        TotalDuration = totalStopwatch.Elapsed,
                        Success = true
                    };

                    // Folosim extensia creată anterior (LoggingExtensions)
                    _logger.LogProductCreationMetrics(metrics);

                    return responseDto;
                }
                catch (Exception ex)
                {
                    totalStopwatch.Stop();

                    var errorMetrics = new ProductCreationMetrics
                    {
                        OperationId = operationId,
                        ProductName = request.Name,
                        SKU = request.SKU,
                        Category = request.Category,
                        ValidationDuration = validationStopwatch.Elapsed,
                        DatabaseSaveDuration = dbStopwatch.Elapsed,
                        TotalDuration = totalStopwatch.Elapsed,
                        Success = false,
                        ErrorReason = ex.Message
                    };
                    _logger.LogProductCreationMetrics(errorMetrics);

                    throw;
                }
            }
        }
    }
}