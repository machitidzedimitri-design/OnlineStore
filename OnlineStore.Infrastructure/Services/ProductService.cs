using OnlineStore.Core.Entities;
using OnlineStore.Core.Exceptions;
using OnlineStore.Core.Interfaces;

namespace OnlineStore.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _uow;

    public ProductService(IProductRepository products, ICategoryRepository categories, IUnitOfWork uow)
    {
        _products = products;
        _categories = categories;
        _uow = uow;
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(int? categoryId = null, CancellationToken ct = default) =>
        _products.GetAllWithCategoryAsync(categoryId, ct);

    public async Task<Product> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _products.GetByIdWithCategoryAsync(id, ct) ?? throw new NotFoundException(nameof(Product), id);

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        await ValidateAsync(product, ct);
        product.Id = 0;
        product.Name = product.Name.Trim();
        await _products.AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(product.Id, ct);
    }

    public async Task<Product> UpdateAsync(int id, Product product, CancellationToken ct = default)
    {
        await ValidateAsync(product, ct);
        var existing = await GetByIdAsync(id, ct);

        existing.Name = product.Name.Trim();
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.StockQuantity = product.StockQuantity;
        existing.CategoryId = product.CategoryId;

        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(id, ct);
        if (await _products.IsUsedInOrdersAsync(id, ct))
            throw new ConflictException("Product is part of existing orders and cannot be deleted.");

        _products.Remove(existing);
        await _uow.SaveChangesAsync(ct);
    }

    private async Task ValidateAsync(Product p, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(p.Name)) throw new BadRequestException("Product name is required.");
        if (p.Name.Trim().Length > 100) throw new BadRequestException("Product name must be at most 100 characters.");
        if ((p.Description ?? string.Empty).Length > 500) throw new BadRequestException("Description must be at most 500 characters.");
        if (p.Price <= 0) throw new BadRequestException("Price must be greater than zero.");
        if (p.StockQuantity < 0) throw new BadRequestException("Stock quantity cannot be negative.");
        if (!await _categories.ExistsAsync(p.CategoryId, ct))
            throw new BadRequestException($"Category with id '{p.CategoryId}' does not exist.");

        p.Description ??= string.Empty;
    }
}
