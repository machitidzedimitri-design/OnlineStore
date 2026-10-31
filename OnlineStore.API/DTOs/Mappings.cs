using OnlineStore.Core.Entities;

namespace OnlineStore.API.DTOs;

public static class Mappings
{
    public static Product ToEntity(this ProductRequest r) => new()
    {
        Name = r.Name, Description = r.Description ?? string.Empty,
        Price = r.Price, StockQuantity = r.StockQuantity, CategoryId = r.CategoryId
    };

    public static Category ToEntity(this CategoryRequest r) => new()
    {
        Name = r.Name, Description = r.Description ?? string.Empty
    };

    public static Customer ToEntity(this CustomerRequest r) => new()
    {
        FirstName = r.FirstName, LastName = r.LastName, Email = r.Email, Phone = r.Phone
    };

    public static ProductResponse ToResponse(this Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.CategoryId, p.Category?.Name);

    public static CategoryResponse ToResponse(this Category c) => new(c.Id, c.Name, c.Description);

    public static CustomerResponse ToResponse(this Customer c) =>
        new(c.Id, c.FirstName, c.LastName, c.Email, c.Phone);

    public static OrderResponse ToResponse(this Order o) => new(
        o.Id, o.CustomerId,
        o.Customer is null ? null : $"{o.Customer.FirstName} {o.Customer.LastName}",
        o.OrderDate, o.Status, o.TotalAmount,
        o.Items.Select(i => new OrderItemResponse(
            i.ProductId, i.Product?.Name, i.Quantity, i.UnitPrice, i.UnitPrice * i.Quantity)).ToList());
}
