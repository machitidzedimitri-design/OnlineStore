namespace OnlineStore.Core.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }

    /// <summary>Product price at the moment the order was created (price snapshot).</summary>
    public decimal UnitPrice { get; set; }
}
