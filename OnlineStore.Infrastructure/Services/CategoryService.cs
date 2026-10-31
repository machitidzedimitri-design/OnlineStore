using OnlineStore.Core.Entities;
using OnlineStore.Core.Exceptions;
using OnlineStore.Core.Interfaces;

namespace OnlineStore.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _uow;

    public CategoryService(ICategoryRepository categories, IUnitOfWork uow)
    {
        _categories = categories;
        _uow = uow;
    }

    public Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default) =>
        _categories.GetAllAsync(ct);

    public async Task<Category> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _categories.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Category), id);

    public async Task<Category> CreateAsync(Category category, CancellationToken ct = default)
    {
        Validate(category);
        category.Id = 0;
        await _categories.AddAsync(category, ct);
        await _uow.SaveChangesAsync(ct);
        return category;
    }

    public async Task<Category> UpdateAsync(int id, Category category, CancellationToken ct = default)
    {
        Validate(category);
        var existing = await GetByIdAsync(id, ct);
        existing.Name = category.Name.Trim();
        existing.Description = category.Description.Trim();
        await _uow.SaveChangesAsync(ct);
        return existing;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(id, ct);
        if (await _categories.HasProductsAsync(id, ct))
            throw new ConflictException("Category contains products and cannot be deleted.");

        _categories.Remove(existing);
        await _uow.SaveChangesAsync(ct);
    }

    private static void Validate(Category c)
    {
        if (string.IsNullOrWhiteSpace(c.Name)) throw new BadRequestException("Category name is required.");
        if (c.Name.Trim().Length > 100) throw new BadRequestException("Category name must be at most 100 characters.");
        if ((c.Description ?? string.Empty).Trim().Length > 300) throw new BadRequestException("Description must be at most 300 characters.");
        c.Description ??= string.Empty;
    }
}
