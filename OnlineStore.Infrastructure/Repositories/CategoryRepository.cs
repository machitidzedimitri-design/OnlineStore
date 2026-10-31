using Microsoft.EntityFrameworkCore;
using OnlineStore.Core.Entities;
using OnlineStore.Core.Interfaces;
using OnlineStore.Infrastructure.Data;

namespace OnlineStore.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(OnlineStoreDbContext db) : base(db) { }

    public override async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
        await Set.AnyAsync(c => c.Id == id, ct);

    public async Task<bool> HasProductsAsync(int id, CancellationToken ct = default) =>
        await Db.Products.AnyAsync(p => p.CategoryId == id, ct);
}
