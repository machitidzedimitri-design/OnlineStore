using System.Net.Mail;
using OnlineStore.Core.Entities;
using OnlineStore.Core.Exceptions;
using OnlineStore.Core.Interfaces;

namespace OnlineStore.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customers;
    private readonly IUnitOfWork _uow;

    public CustomerService(ICustomerRepository customers, IUnitOfWork uow)
    {
        _customers = customers;
        _uow = uow;
    }

    public Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default) =>
        _customers.GetAllAsync(ct);

    public async Task<Customer> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _customers.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Customer), id);

    public async Task<Customer> CreateAsync(Customer customer, CancellationToken ct = default)
    {
        Validate(customer);
        if (await _customers.EmailExistsAsync(customer.Email, null, ct))
            throw new ConflictException($"A customer with email '{customer.Email}' already exists.");

        customer.Id = 0;
        await _customers.AddAsync(customer, ct);
        await _uow.SaveChangesAsync(ct);
        return customer;
    }

    public async Task<Customer> UpdateAsync(int id, Customer customer, CancellationToken ct = default)
    {
        Validate(customer);
        var existing = await GetByIdAsync(id, ct);

        if (await _customers.EmailExistsAsync(customer.Email, id, ct))
            throw new ConflictException($"A customer with email '{customer.Email}' already exists.");

        existing.FirstName = customer.FirstName;
        existing.LastName = customer.LastName;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;

        await _uow.SaveChangesAsync(ct);
        return existing;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(id, ct);
        if (await _customers.HasOrdersAsync(id, ct))
            throw new ConflictException("Customer has orders and cannot be deleted.");

        _customers.Remove(existing);
        await _uow.SaveChangesAsync(ct);
    }

    private static void Validate(Customer c)
    {
        c.FirstName = (c.FirstName ?? string.Empty).Trim();
        c.LastName = (c.LastName ?? string.Empty).Trim();
        c.Email = (c.Email ?? string.Empty).Trim();
        c.Phone = string.IsNullOrWhiteSpace(c.Phone) ? null : c.Phone.Trim();

        if (c.FirstName.Length == 0) throw new BadRequestException("First name is required.");
        if (c.LastName.Length == 0) throw new BadRequestException("Last name is required.");
        if (c.FirstName.Length > 50 || c.LastName.Length > 50)
            throw new BadRequestException("First and last name must be at most 50 characters.");
        if (c.Email.Length == 0 || c.Email.Length > 150 || !IsValidEmail(c.Email))
            throw new BadRequestException("Email is missing or has an invalid format.");
        if (c.Phone is { Length: > 30 }) throw new BadRequestException("Phone must be at most 30 characters.");
    }

    private static bool IsValidEmail(string email) =>
        MailAddress.TryCreate(email, out var addr) && addr.Address == email && email.Contains('.', StringComparison.Ordinal);
}
