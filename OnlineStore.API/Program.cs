using Microsoft.EntityFrameworkCore;
using OnlineStore.API.Middleware;
using OnlineStore.Core.Interfaces;
using OnlineStore.Infrastructure.Data;
using OnlineStore.Infrastructure.Repositories;
using OnlineStore.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Framework services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- EF Core + SQL Server (connection string lives in appsettings.json) ---
builder.Services.AddDbContext<OnlineStoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Dependency Injection: depend on abstractions (DIP) ---
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OnlineStoreDbContext>());

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// --- Typed HttpClient for the external exchange-rate API ---
builder.Services.AddHttpClient<ICurrencyConverter, ExchangeRateApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExchangeRateApi:BaseUrl"] ?? "https://open.er-api.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

// Must be first so it can catch exceptions from everything after it.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Apply migrations automatically in development (creates OnlineStoreDb on first run).
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OnlineStoreDbContext>();
    await db.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
