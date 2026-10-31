using Microsoft.EntityFrameworkCore;
using OnlineStore.Core.Entities;
using OnlineStore.Core.Interfaces;
using OnlineStore.Infrastructure.Data;

namespace OnlineStore.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(OnlineStoreDbContext db) : base(db) { }

    public override async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking().OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync(ct);

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
        await Set.AnyAsync(c => c.Id == id, ct);

    public async Task<bool> HasOrdersAsync(int id, CancellationToken ct = default) =>
        await Db.Orders.AnyAsync(o => o.CustomerId == id, ct);

    public async Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLower();
        return await Set.AnyAsync(
            c => c.Email.ToLower() == normalized && (excludeCustomerId == null || c.Id != excludeCustomerId), ct);
    }
}
