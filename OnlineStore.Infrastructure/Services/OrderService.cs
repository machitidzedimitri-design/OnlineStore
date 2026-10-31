using OnlineStore.Core.Entities;
using OnlineStore.Core.Exceptions;
using OnlineStore.Core.Interfaces;
using OnlineStore.Core.Models;

namespace OnlineStore.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orders;
    private readonly IProductRepository _products;
    private readonly ICustomerRepository _customers;
    private readonly IUnitOfWork _uow;

    public OrderService(IOrderRepository orders, IProductRepository products,
                        ICustomerRepository customers, IUnitOfWork uow)
    {
        _orders = orders;
        _products = products;
        _customers = customers;
        _uow = uow;
    }

    public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default) =>
        _orders.GetAllWithDetailsAsync(ct);

    public async Task<Order> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _orders.GetByIdWithDetailsAsync(id, ct) ?? throw new NotFoundException(nameof(Order), id);

    public async Task<Order> CreateAsync(int customerId, IReadOnlyCollection<OrderLine> lines, CancellationToken ct = default)
    {
        if (lines is null || lines.Count == 0)
            throw new BadRequestException("An order must contain at least one item.");
        if (lines.Any(l => l.Quantity <= 0))
            throw new BadRequestException("Item quantity must be greater than zero.");

        if (!await _customers.ExistsAsync(customerId, ct))
            throw new NotFoundException(nameof(Customer), customerId);

        // Merge duplicate product lines: [{1,2},{1,3}] -> {1,5}
        var merged = lines.GroupBy(l => l.ProductId)
                          .Select(g => new OrderLine(g.Key, g.Sum(x => x.Quantity)))
                          .ToList();

        var products = await _products.GetByIdsAsync(merged.Select(m => m.ProductId), ct);
        var productMap = products.ToDictionary(p => p.Id);

        var missing = merged.Where(m => !productMap.ContainsKey(m.ProductId)).Select(m => m.ProductId).ToList();
        if (missing.Count > 0)
            throw new NotFoundException($"Product(s) not found: {string.Join(", ", missing)}.");

        var order = new Order
        {
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatuses.Pending
        };

        foreach (var line in merged)
        {
            var product = productMap[line.ProductId];
            if (product.StockQuantity < line.Quantity)
                throw new ConflictException(
                    $"Insufficient stock for '{product.Name}': requested {line.Quantity}, available {product.StockQuantity}.");

            product.StockQuantity -= line.Quantity;
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = line.Quantity,
                UnitPrice = product.Price // price snapshot
            });
        }

        // Total is always calculated on the server; client-provided totals are never trusted.
        order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        await _orders.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct); // single SaveChanges = one atomic transaction (order + stock)
        return await GetByIdAsync(order.Id, ct);
    }

    public async Task<Order> UpdateStatusAsync(int id, string status, CancellationToken ct = default)
    {
        if (!OrderStatuses.IsValid(status))
            throw new BadRequestException($"Invalid status. Allowed values: {string.Join(", ", OrderStatuses.All)}.");

        var order = await GetByIdAsync(id, ct);
        var newStatus = OrderStatuses.Normalize(status);

        if (order.Status is OrderStatuses.Cancelled or OrderStatuses.Delivered && order.Status != newStatus)
            throw new ConflictException($"Order is already {order.Status} and its status cannot be changed.");

        if (newStatus == OrderStatuses.Cancelled && order.Status != OrderStatuses.Cancelled)
            RestoreStock(order);

        order.Status = newStatus;
        await _uow.SaveChangesAsync(ct);
        return order;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var order = await GetByIdAsync(id, ct);

        if (order.Status is not (OrderStatuses.Pending or OrderStatuses.Cancelled))
            throw new ConflictException($"Orders with status '{order.Status}' cannot be deleted. Only Pending or Cancelled orders can.");

        if (order.Status == OrderStatuses.Pending)
            RestoreStock(order);

        _orders.Remove(order);
        await _uow.SaveChangesAsync(ct);
    }

    private static void RestoreStock(Order order)
    {
        foreach (var item in order.Items)
            if (item.Product is not null)
                item.Product.StockQuantity += item.Quantity;
    }
}
