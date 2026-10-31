using Microsoft.EntityFrameworkCore;
using OnlineStore.Core.Entities;
using OnlineStore.Core.Interfaces;
using OnlineStore.Infrastructure.Data;

namespace OnlineStore.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(OnlineStoreDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Product>> GetAllWithCategoryAsync(int? categoryId, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking().Include(p => p.Category).AsQueryable();
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        return await query.OrderBy(p => p.Name).ToListAsync(ct);
    }

    public async Task<Product?> GetByIdWithCategoryAsync(int id, CancellationToken ct = default) =>
        await Set.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        return await Set.Where(p => idList.Contains(p.Id)).ToListAsync(ct);
    }

    public async Task<bool> IsUsedInOrdersAsync(int productId, CancellationToken ct = default) =>
        await Db.OrderItems.AnyAsync(i => i.ProductId == productId, ct);
}
