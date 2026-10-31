namespace OnlineStore.Core.Entities;

public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlyList<string> All =
        new[] { Pending, Confirmed, Shipped, Delivered, Cancelled };

    public static bool IsValid(string? status) =>
        status is not null && All.Contains(status, StringComparer.OrdinalIgnoreCase);

    public static string Normalize(string status) =>
        All.First(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));
}
