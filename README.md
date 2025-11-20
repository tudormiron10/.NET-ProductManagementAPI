# Product Management API - Advanced .NET Exercise

This repository contains the implementation of the **Advanced .NET Product Management Exercise**. 
The solution focuses on **Vertical Slice Architecture**, **CQRS**, **Complex Validation**, and **High-Performance Structured Logging**.

It strictly follows the requirements checklist, ensuring robust error handling, domain-centric validation, and comprehensive integration test coverage.

---

## 🚀 Tech Stack & Patterns

* **Framework:** .NET 8 (Web API)
* **Architecture:** Vertical Slice Architecture (Feature-based organization)
* **Command Handling:** MediatR (CQRS Pattern)
* **Mapping:** AutoMapper (Advanced patterns: Custom Resolvers, Conditional Mapping)
* **Validation:** FluentValidation (Async rules, Cross-field validation) & Data Annotations
* **Database:** Entity Framework Core (In-Memory for isolation)
* **Testing:** xUnit & Moq
* **Telemetry:** Structured Logging (`LoggerMessage` pattern) & Correlation Middleware

---

## 📂 Project Structure

The project moves away from traditional Layered Architecture (Controller -> Service -> Repo) in favor of cohesive **Vertical Slices**. All logic related to "Products" lives in one feature folder.

```text
Product Management/
├── Product Management API/
│   ├── Features/
│   │   └── Products/          # ➤ VERTICAL SLICE (Domain Logic)
│   │       ├── DTOs/          # Request/Response contracts
│   │       ├── CreateProductHandler.cs  # The "Brain" (CQRS Handler)
│   │       ├── Product.cs     # Domain Entity
│   │       └── ProductCategory.cs
│   │
│   ├── Common/
│   │   ├── Logging/           # Telemetry, Metrics, LogEvents
│   │   ├── Mapping/           # AutoMapper Profile & Custom Resolvers
│   │   └── Middleware/        # Correlation ID Middleware
│   │
│   ├── Validators/            # FluentValidation & Custom Attributes
│   ├── Data/                  # EF Core Context (InMemory)
│   └── Program.cs             # DI Container & Pipeline Configuration
│
└── Product_Management_API.Tests/
    └── CreateProductHandlerIntegrationTests.cs # Integration Tests

## ✅ Key Features Implemented

### 🛠️ Module 1: Advanced AutoMapper
* **Custom Value Resolvers:** Encapsulated domain logic for calculated fields:
  * `ProductAgeResolver`: Determines if a product is a **"New Release"** or **"Classic"**.
  * `BrandInitialsResolver`: Parses brand names into initials (e.g., "New Balance" → "NB").
  * `AvailabilityStatusResolver`: Derives status text based on exact stock levels.
* **Conditional Mapping:**
  * **Price Logic:** Automatically applies a **10% discount** for the *Home* category.
  * **Content Filtering:** Forces `ImageUrl` to `null` for the *Home* category.

### 📊 Module 2: Structured Logging & Telemetry
* **Performance Tracking:** Measures high-precision execution time for:
  * Validation Phase
  * Database Persistence
  * Total Operation Duration
* **Correlation ID:** Middleware ensures end-to-end traceability via the `X-Correlation-ID` header.
* **Structured Logging:** Utilizes high-performance `LoggerMessage` delegates and defined `LogEvents` constants (e.g., `2001`, `2003`).
* **Metrics:** Logs a comprehensive, JSON-structure metric object upon operation completion.

### 🛡️ Module 3: Complex Validation
* **Async Validation:** Performs non-blocking database checks for:
  * **SKU Uniqueness**
  * **Name + Brand Uniqueness**
* **Business Rules:**
  * 📱 **Electronics:** Min price **$50**, must be released within the last **5 years**.
  * 🏠 **Home:** Max price **$200**, strict content filtering for inappropriate words.
  * 📉 **Cross-field:** Expensive items (**>$100**) are restricted to low stock levels.
* **Client-Side Support:** Implemented `ValidSKUAttribute` to support MVC/Front-end validation adapters.

### 🧪 Module 4: Integration & Testing
* **xUnit Integration Tests:** 3 comprehensive scenarios covering:
  1.  **Success Flow:** Verifies AutoMapper resolvers, persistence, and DTO shaping.
  2.  **Validation Failure:** Verifies Exception handling and Warning logs for duplicate SKUs.
  3.  **Conditional Logic:** Verifies specific rules for *Home* category discounts and filtering.
* **Infrastructure:** Utilizes unique **In-Memory Database** instances per test run to ensure complete data isolation.

🛠️ How to Run
1. Build the Solution
Bash

dotnet build
2. Run the API
Bash

cd "Product Management API"
dotnet run
The API will start (usually on https://localhost:7xxx). Access Swagger UI at /swagger.

3. Run Tests
To verify all requirements and logic:

cd "../Product_Management_API.Tests"
dotnet test
Expected Output: Passed! (Total: 3)

📝 API Usage
POST /products
Creates a new product profile.

Sample Request:
JSON
  {
    "name": "Noise Cancelling Headphones",
    "brand": "Sony",
    "sku": "SONY-WH-1000",
    "category": 0,
    "price": 299.99,
    "releaseDate": "2024-01-01T00:00:00Z",
    "stockQuantity": 50,
    "imageUrl": "[http://example.com/img.jpg](http://example.com/img.jpg)"
  }
Sample Response (201 Created):
JSON
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Noise Cancelling Headphones",
    "brand": "Sony",
    "sku": "SONY-WH-1000",
    "categoryDisplayName": "Electronics & Technology",
    "price": 299.99,
    "formattedPrice": "$299.99",
    "productAge": "New Release",
    "brandInitials": "S",
    "availabilityStatus": "In Stock"
  }
