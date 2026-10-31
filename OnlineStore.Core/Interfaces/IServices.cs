using OnlineStore.Core.Entities;
using OnlineStore.Core.Models;

namespace OnlineStore.Core.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllAsync(int? categoryId = null, CancellationToken ct = default);
    Task<Product> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task<Product> UpdateAsync(int id, Product product, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);
    Task<Category> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Category> CreateAsync(Category category, CancellationToken ct = default);
    Task<Category> UpdateAsync(int id, Category category, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface ICustomerService
{
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default);
    Task<Customer> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Customer> CreateAsync(Customer customer, CancellationToken ct = default);
    Task<Customer> UpdateAsync(int id, Customer customer, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IOrderService
{
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default);
    Task<Order> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Order> CreateAsync(int customerId, IReadOnlyCollection<OrderLine> lines, CancellationToken ct = default);
    Task<Order> UpdateStatusAsync(int id, string status, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}


public interface ICurrencyConverter
{
    
    Task<decimal> GetRateFromUsdAsync(string currencyCode, CancellationToken ct = default);
}
