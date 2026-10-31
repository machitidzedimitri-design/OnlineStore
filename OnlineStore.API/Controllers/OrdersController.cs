using Microsoft.AspNetCore.Mvc;
using OnlineStore.API.DTOs;
using OnlineStore.Core.Interfaces;
using OnlineStore.Core.Models;

namespace OnlineStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;
    private readonly ICurrencyConverter _currency;

    public OrdersController(IOrderService orders, ICurrencyConverter currency)
    {
        _orders = orders;
        _currency = currency;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAll(CancellationToken ct) =>
        Ok((await _orders.GetAllAsync(ct)).Select(o => o.ToResponse()));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id, CancellationToken ct) =>
        Ok((await _orders.GetByIdAsync(id, ct)).ToResponse());

    /// <summary>Create an order. The total is calculated on the server from current product prices.</summary>
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var lines = request.Items.Select(i => new OrderLine(i.ProductId, i.Quantity)).ToList();
        var created = await _orders.CreateAsync(request.CustomerId, lines, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToResponse());
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<OrderResponse>> UpdateStatus(int id, UpdateOrderStatusRequest request, CancellationToken ct) =>
        Ok((await _orders.UpdateStatusAsync(id, request.Status, ct)).ToResponse());

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _orders.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>Order total converted from USD using an external exchange-rate API (HttpClient).</summary>
    [HttpGet("{id:int}/converted")]
    public async Task<ActionResult<ConvertedTotalResponse>> GetConvertedTotal(int id, [FromQuery] string currency, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(id, ct);
        var rate = await _currency.GetRateFromUsdAsync(currency, ct);
        return Ok(new ConvertedTotalResponse(order.Id, order.TotalAmount, currency.Trim().ToUpperInvariant(),
                                             rate, Math.Round(order.TotalAmount * rate, 2)));
    }
}
