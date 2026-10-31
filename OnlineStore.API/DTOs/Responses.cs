namespace OnlineStore.API.DTOs;

public record ProductResponse(int Id, string Name, string Description, decimal Price,
                              int StockQuantity, int CategoryId, string? CategoryName);

public record CategoryResponse(int Id, string Name, string Description);

public record CustomerResponse(int Id, string FirstName, string LastName, string Email, string? Phone);

public record OrderItemResponse(int ProductId, string? ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

public record OrderResponse(int Id, int CustomerId, string? CustomerName, DateTime OrderDate,
                            string Status, decimal TotalAmount, IReadOnlyList<OrderItemResponse> Items);

public record ConvertedTotalResponse(int OrderId, decimal TotalUsd, string Currency, decimal Rate, decimal ConvertedTotal);
