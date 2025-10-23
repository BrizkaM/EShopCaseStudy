# E-Shop REST API

REST API service for managing e-shop products.

## Technologies

- .NET 8.0 (LTS)
- ASP.NET Core Web API
- Entity Framework Core
- Swagger/OpenAPI
- MSTest + Moq (Unit Tests)
- In-Memory Queue
- API Versioning (URL-based)

## Features

- **CRUD Operations** for products
- **API Versioning** (v1 and v2)
- **Pagination** in API v2 (default 10 items/page, max 100)
- **Asynchronous queue** for stock updates in v2 (processed every 2 seconds)
- **Swagger documentation** for both versions
- **Unit tests** with average coverage (50+ tests using MSTest)
- **Repository Pattern + Unit of Work**
- **SOLID principles**
- **Clean Architecture** (separate Queue project)
- **MSSQL database** with migrations
- **Database seeding** (5 sample products)
- **Logging** (structured logging with ILogger)
- **Error handling** with ApiResponse wrapper
- **Data validation** (Data Annotations)

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Prefered IDE Visual Studio 2022 - but any IDE can be used (VS Code, Rider)

## Recommended quick steps

- Open solution EshopProject in Visual Studio 2022
- Build solution
- run tests in Test Explorer
- set-up EShopProject.WebApi as Startup project
- run https (database auto-created, swagger UI should be opened)

## Project Structure

```
EShopProject/
├── EShopProject.WebApi/          # Web API project
├── EShopProject.Core/            # entity - model, interfaces
├── EShopProject.Services/        # Business logic, services, inputs outputs
├── EShopProject.EShopDB/         # Data access, repositories, migrations
├── EShopProject.Queue/           # InMemory queue infrastructure
Tests folder:
    └── EShopProject.ServicesIntegrationTest/       # Unit integration tests (MSTest)
    └── EShopProject.WebApiTest/                    # Unit tests (MSTest)
```

## Running the Application

### 1. Clone the repository
```bash
git clone https://github.com/BrizkaM/EShopCaseStudy.git
cd EShopCaseStudy
```

### 2. Restore dependencies
```bash
dotnet restore
```

### 3. Run database migrations
```bash
cd EShopProject.EShopDB
dotnet ef database update
```

### 4. Run the application
```bash
dotnet run --project EShopProject.WebApi
```

The API will be available at:
- HTTPS: `https://localhost:7287`
- HTTP: `http://localhost:5265`
- Swagger UI: `https://localhost:7287/swagger`

## Running Unit Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## API Documentation

After running the application, navigate to `https://localhost:7001/swagger` to access interactive API documentation.

## API Endpoints

### Version 1 (Basic CRUD)

- `GET /api/v1/products` - Get all products (no pagination)
- `GET /api/v1/products/{id}` - Get product by ID
- `POST /api/v1/products` - Create new product
- `PATCH /api/v1/products/{id}/stock` - Update product stock (synchronous)

### Version 2 (With Pagination & Async Queue)

- `GET /api/v2/products?pageNumber=1&pageSize=10` - Get paginated products (**default: 10 per page**)
- `GET /api/v2/products/{id}` - Get product by ID
- `POST /api/v2/products` - Create new product
- `PATCH /api/v2/products/{id}/stock` - Update product stock (**asynchronous via queue**)

### Create Product
```bash
curl -X POST https://localhost:7287/api/v1/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Gaming Laptop",
    "imageUrl": "https://example.com/laptop.jpg"
  }'
```

### Get All Products
```bash
curl https://localhost:7287/api/v1/products
```

### Update Stock (V1 - Synchronous)
```bash
curl -X PATCH https://localhost:7287/api/v1/products/1/stock \
  -H "Content-Type: application/json" \
  -d '{"quantity": 50}'
```

### Get Paginated Products (V2)
```bash
curl "https://localhost:7287/api/v2/products?pageNumber=1&pageSize=10"
```

### Update Stock (V2 - Asynchronous via Queue)
```bash
curl -X PATCH https://localhost:7287/api/v2/products/1/stock \
  -H "Content-Type: application/json" \
  -d '{"quantity": 50}'
```

## Database

The application uses SqlServer database (`EShopDB.db`). The database is automatically created and seeded with sample data on first run.

## Asynchronous Queue

Version 2 uses an in-memory queue for processing stock updates asynchronously.

## License

MIT

## Author

Martin Brezina
