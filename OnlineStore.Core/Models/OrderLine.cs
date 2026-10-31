namespace OnlineStore.Core.Models;

/// <summary>A single requested line when creating an order.</summary>
public record OrderLine(int ProductId, int Quantity);
