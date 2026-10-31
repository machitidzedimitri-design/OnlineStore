using Microsoft.EntityFrameworkCore;
using OnlineStore.Core.Entities;
using OnlineStore.Core.Interfaces;
using OnlineStore.Infrastructure.Data;

namespace OnlineStore.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(OnlineStoreDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Order>> GetAllWithDetailsAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking()
                 .Include(o => o.Customer)
                 .Include(o => o.Items).ThenInclude(i => i.Product)
                 .OrderByDescending(o => o.OrderDate)
                 .ToListAsync(ct);

    // Tracked on purpose: services modify the order (status, stock) and save.
    public async Task<Order?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
        await Set.Include(o => o.Customer)
                 .Include(o => o.Items).ThenInclude(i => i.Product)
                 .FirstOrDefaultAsync(o => o.Id == id, ct);
}
