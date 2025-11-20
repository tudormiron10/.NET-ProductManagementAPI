using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Product_Management_API.Common.Logging;
using Product_Management_API.Data;
using Product_Management_API.Features.Products;
using Product_Management_API.Mappings;


namespace Product_Management_API.Tests
{
    public class CreateProductHandlerIntegrationTests : IDisposable
    {
        private readonly ApplicationContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly Mock<ILogger<CreateProductHandler>> _loggerMock;
        private readonly CreateProductHandler _handler;

        public CreateProductHandlerIntegrationTests()
        {
            // Set up in-memory database with unique name per test run
            var dbOptions = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;
            
            _context = new ApplicationContext(dbOptions);

            // Configure AutoMapper with the actual profile
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AdvancedProductMappingProfile());
            });
            _mapper = mappingConfig.CreateMapper();

            // Set up memory cache
            _cache = new MemoryCache(new MemoryCacheOptions());

            // Mock Logger
            _loggerMock = new Mock<ILogger<CreateProductHandler>>();

            // Create handler instance
            _handler = new CreateProductHandler(_context, _mapper, _loggerMock.Object, _cache);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _cache.Dispose();
        }

        [Fact]
        public async Task Handle_ValidElectronicsProductRequest_CreatesProductWithCorrectMappings()
        {
            // Arrange
            var request = new CreateProductProfileRequest
            {
                Name = "Noise Cancelling Headphones",
                Brand = "New Balance",
                SKU = "NB-HEAD-001",
                Category = ProductCategory.Electronics,
                Price = 100m,
                ReleaseDate = DateTime.UtcNow.AddDays(-10),
                StockQuantity = 1,
                ImageUrl = "http://test.com/img.jpg"
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ProductProfileDTO>(result);
            
            // Check Resolvers
            Assert.Equal("Electronics & Technology", result.CategoryDisplayName);
            Assert.Equal("NB", result.BrandInitials);
            Assert.Equal("New Release", result.ProductAge);
            Assert.Equal("Last Item", result.AvailabilityStatus);
            Assert.False(string.IsNullOrWhiteSpace(result.FormattedPrice));

            // Verify Logging
            VerifyLoggerCall(LogEvents.ProductCreationStarted, Times.Once());
        }

        [Fact]
        public async Task Handle_DuplicateSKU_ThrowsValidationExceptionWithLogging()
        {
            // Arrange: Create existing product
            var existingProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Existing",
                Brand = "Test",
                SKU = "DUPLICATE-SKU",
                Category = ProductCategory.Books
            };
            _context.Products.Add(existingProduct);
            await _context.SaveChangesAsync();

            // Arrange: Request with same SKU
            var request = new CreateProductProfileRequest
            {
                Name = "New Product",
                Brand = "Test",
                SKU = "DUPLICATE-SKU",
                Category = ProductCategory.Books
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(request, CancellationToken.None));

            Assert.Contains("already exists", exception.Message);
            
            // Verify Warning Log
            VerifyLoggerCallWarning(LogEvents.ProductValidationFailed, Times.Once());
        }

        [Fact]
        public async Task Handle_HomeProductRequest_AppliesDiscountAndConditionalMapping()
        {
            // Arrange
            var request = new CreateProductProfileRequest
            {
                Name = "Garden Chair",
                Brand = "HomeDepot",
                SKU = "HOME-001",
                Category = ProductCategory.Home,
                Price = 200m,
                StockQuantity = 10,
                ImageUrl = "http://should-be-null.com/img.jpg"
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal("Home & Garden", result.CategoryDisplayName);
            Assert.Equal(180m, result.Price); // 10% discount (200 * 0.9)
            Assert.Null(result.ImageUrl); // Content filtering
        }

        // Helper: Verify Information Logs
        private void VerifyLoggerCall(int eventId, Times times)
        {
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.Is<EventId>(e => e.Id == eventId),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times);
        }

        // Helper: Verify Warning Logs
        private void VerifyLoggerCallWarning(int eventId, Times times)
        {
             _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.Is<EventId>(e => e.Id == eventId),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times);
        }
    }
}