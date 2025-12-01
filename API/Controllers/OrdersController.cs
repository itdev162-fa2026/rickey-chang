using System.ComponentModel.DataAnnotations;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Stripe.Checkout;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly DataContext _context;

    public OrdersController(DataContext context)
    {
        _context = context;
    }

    // GET: api/orders/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    // GET: api/orders/session/cs_test_xxxxx
    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<Order>> GetOrderBySessionId(string sessionId)
    {
        var sessionService = new SessionService();
        Session stripeSession;

        try
        {
            stripeSession = await sessionService.GetAsync(sessionId);
        }
        catch (Stripe.StripeException ex)
        {
            return BadRequest($"Invalid session ID: {ex.Message}");
        }

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.StripeSessionId == sessionId);

        if (order == null)
        {
            return NotFound("Order not found");
        }

        // Sync status from Stripe
        if (stripeSession.PaymentStatus == "paid" && order.Status != OrderStatus.Completed)
        {
            order.Status = OrderStatus.Completed;
            order.CompletedDate = DateTime.Now;
            order.StripePaymentIntentId = stripeSession.PaymentIntentId;
            await _context.SaveChangesAsync();
        }
        else if (stripeSession.PaymentStatus == "unpaid" && order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Failed;
            await _context.SaveChangesAsync();
        }

        return Ok(order);
    }

    // POST: api/orders
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest("Cart is empty");
        }

        var order = new Order
        {
            CustomerEmail = request.CustomerEmail,
            Status = OrderStatus.Pending,
            CreatedDate = DateTime.Now,
            OrderItems = new List<OrderItem>()
        };

        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                return BadRequest($"Product with ID {item.ProductId} not found");
            }

            var priceToUse = product.IsOnSale ? product.SalePrice!.Value : product.Price;

            order.OrderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                PriceAtPurchase = priceToUse
            });

            totalAmount += priceToUse * item.Quantity;
        }

        order.TotalAmount = totalAmount;
        order.Status = OrderStatus.Completed; // temporary for Activity 11
        order.CompletedDate = DateTime.Now;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }
}


// Request model for creating orders
public class CreateOrderRequest
{
    [Required]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    public List<CartItemRequest> Items { get; set; } = new();
}

public class CartItemRequest
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}