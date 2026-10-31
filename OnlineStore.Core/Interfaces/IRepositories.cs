using OnlineStore.Core.Entities;

namespace OnlineStore.Core.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetAllWithCategoryAsync(int? categoryId, CancellationToken ct = default);
    Task<Product?> GetByIdWithCategoryAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<bool> IsUsedInOrdersAsync(int productId, CancellationToken ct = default);
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<bool> HasProductsAsync(int id, CancellationToken ct = default);
}

public interface ICustomerRepository : IRepository<Customer>
{
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<bool> HasOrdersAsync(int id, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null, CancellationToken ct = default);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<IReadOnlyList<Order>> GetAllWithDetailsAsync(CancellationToken ct = default);
    Task<Order?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
}
