# Online Store Management System (ASP.NET Core Web API)

Final project for the C# / .NET course: a layered REST API for managing products, categories,
customers and orders, backed by SQL Server through Entity Framework Core.

## Technologies
- .NET 8 / ASP.NET Core Web API (controllers)
- Entity Framework Core 8 + MS SQL Server
- Swagger UI (Swashbuckle)
- Typed `HttpClient` for an external exchange-rate API (https://open.er-api.com)

## Architecture
```
OnlineStore.API              Controllers, DTOs (requests/responses), exception middleware, Program.cs
OnlineStore.Core             Entities, interfaces (repositories/services), custom exceptions
OnlineStore.Infrastructure   DbContext, repository + service implementations, HttpClient client
Database/OnlineStoreDb.sql   Optional SQL script (tables, constraints, view, stored procedure)
```
Dependencies point inward: API -> Infrastructure -> Core, and controllers/services only know interfaces.

### SOLID in this project
| Principle | Where |
|---|---|
| Single Responsibility | Controllers handle HTTP only; services hold business rules; repositories do data access; middleware handles errors |
| Open/Closed | New behaviour = new service/repository class registered in DI; existing classes unchanged. Generic `Repository<T>` is extended by `ProductRepository` etc. |
| Liskov Substitution | Any `IProductRepository` / `IOrderService` implementation (e.g. in-memory fake) can replace the EF one without changing callers |
| Interface Segregation | Small, focused interfaces: `IUnitOfWork`, `IRepository<T>`, one service interface per aggregate, `ICurrencyConverter` |
| Dependency Inversion | Controllers depend on `IProductService`, services on `I*Repository`; everything is wired in `Program.cs` via DI |

### Async & errors
- All data access uses `ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`, `FindAsync`, `SaveChangesAsync` with `CancellationToken`.
- Services throw typed exceptions (`NotFoundException`, `BadRequestException`, `ConflictException`, `ExternalServiceException`);
  `ExceptionHandlingMiddleware` maps them centrally to 404 / 400 / 409 / 502 `ProblemDetails`. No repeated try/catch.
- DTO validation (DataAnnotations) returns 400 automatically; the service layer re-checks business rules.

## Business rules
- Product: name required, price > 0, stock >= 0, category must exist. Products used in orders cannot be deleted.
- Category: cannot be deleted while it has products.
- Customer: names required, valid and **unique** email (409 on duplicate). Customers with orders cannot be deleted.
- Order creation: customer and every product must exist, quantities > 0, stock must be sufficient (otherwise nothing is created, 409).
  Duplicate product lines are merged. `UnitPrice` is a snapshot of the price at order time. **`TotalAmount` is computed on the server**
  (the request has no total field). Stock is decremented in the same `SaveChanges` (atomic).
- Order status: `Pending, Confirmed, Shipped, Delivered, Cancelled`. Cancelling restores stock; Cancelled/Delivered are final.
  Only `Pending` or `Cancelled` orders can be deleted (Pending ones restore stock).

## Endpoints
| Resource | Endpoints |
|---|---|
| Products | `GET /api/products[?categoryId=]`, `GET /api/products/{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` |
| Categories | `GET /api/categories`, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` |
| Customers | `GET /api/customers`, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` |
| Orders | `GET /api/orders`, `GET /{id}`, `POST`, `PUT /{id}/status`, `DELETE /{id}`, `GET /{id}/converted?currency=EUR` |

## Getting started
Requirements: .NET 8 SDK, SQL Server (LocalDB, Express, or Docker).

1. Check the connection string in `OnlineStore.API/appsettings.json` (default: LocalDB).
   Docker example: `Server=localhost,1433;Database=OnlineStoreDb;User Id=sa;Password=<your password>;TrustServerCertificate=True`
2. Install the EF tool and create the initial migration (once):
   ```bash
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate -p OnlineStore.Infrastructure -s OnlineStore.API -o Data/Migrations
   ```
3. Run the API (the database is created/migrated automatically in Development):
   ```bash
   dotnet run --project OnlineStore.API
   ```
4. Open Swagger UI at http://localhost:5080/swagger, or use `OnlineStore.API/OnlineStore.http` (VS / VS Code REST Client / Rider).

To apply migrations manually: `dotnet ef database update -p OnlineStore.Infrastructure -s OnlineStore.API`.

**Alternative:** run `Database/OnlineStoreDb.sql` in SSMS instead of migrations. In that case remove the
`MigrateAsync()` call in `Program.cs`.

## Demo checklist
1. CRUD for categories, products, customers.
2. Create an order and show server-calculated total + reduced stock.
3. Show errors: 400 (invalid product), 404 (unknown id), 409 (duplicate email / insufficient stock / delete used product).
4. Change order status; cancel to see stock restored.
5. Call `/api/orders/{id}/converted?currency=EUR` (external API).
