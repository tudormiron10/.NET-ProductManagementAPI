using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Product_Management_API.Common.Middleware;
using Product_Management_API.Data;
using Product_Management_API.Features.Products;
using Product_Management_API.Mappings;
using Product_Management_API.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseInMemoryDatabase("ProductDb"));

builder.Services.AddAutoMapper(typeof(AdvancedProductMappingProfile).Assembly);

// 3. MediatR Registration (Necesar pentru Handler)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddScoped<IValidator<CreateProductProfileRequest>, CreateProductProfileValidator>();

// "Register all validators from assembly"
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductProfileValidator>();

// 5. Memory Cache Registration (Necesar pentru Handler - Task 1.3)
builder.Services.AddMemoryCache();

// Add Services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// "Add CorrelationMiddleware to pipeline"
app.UseMiddleware<CorrelationMiddleware>();

app.MapPost("/products", async (
    [FromBody] CreateProductProfileRequest request,
    [FromServices] IMediator mediator) =>
{
    // Handler-ul returneaza ProductProfileDTO
    var result = await mediator.Send(request);
    
    // Returneaza 201 Created
    return Results.Created($"/products/{result.Id}", result);
})
.WithName("CreateProduct")
.WithTags("Products") // Documentare specifică produselor în Swagger
.Produces<ProductProfileDTO>(201)
.Produces(400);

app.Run();