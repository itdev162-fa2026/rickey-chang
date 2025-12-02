using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Stripe.Checkout;
using System.ComponentModel.DataAnnotations;

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

    // GET: api/orders/session/cs_test_xxxxx
    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<Order>> GetOrderBySessionId(string sessionId)
{
    // Fetch the session from Stripe to get payment status
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

    // Find order in our database
    var order = await _context.Orders
        .Include(o => o.OrderItems)
        .FirstOrDefaultAsync(o => o.StripeSessionId == sessionId);

    if (order == null)
    {
        return NotFound("Order not found");
    }

    // Update order status based on Stripe payment status
    if (stripeSession.PaymentStatus == "paid" && order.Status != OrderStatus.Completed)
    {
        order.Status = OrderStatus.Completed;
        order.CompletedDate = DateTime.Now;
        order.StripePaymentIntentId = stripeSession.PaymentIntentId;
        await _context.SaveChangesAsync();
    }
    else if (stripeSession.PaymentStatus == "unpaid" && order.Status == OrderStatus.Pending)
    {
        // Payment was not completed
        order.Status = OrderStatus.Failed;
        await _context.SaveChangesAsync();
    }

    return Ok(order);
}

    // GET: api/orders/5
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

        // Create order
        var order = new Order
        {
            CustomerEmail = request.CustomerEmail,
            Status = OrderStatus.Pending,
            CreatedDate = DateTime.Now
        };

        // Process each cart item
        decimal totalAmount = 0;
        foreach (var item in request.Items)
        {
            // Verify product exists
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                return BadRequest($"Product with ID {item.ProductId} not found");
            }

            // SECURITY: Always use current price from database, never trust client
            var priceToUse = product.IsOnSale ? product.SalePrice!.Value : product.Price;

            // Create order item with point-in-time data
            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                PriceAtPurchase = priceToUse
            };

            order.OrderItems.Add(orderItem);
            totalAmount += priceToUse * item.Quantity;
        }

        order.TotalAmount = totalAmount;

        // For now, mark as completed immediately (Activity 11 will use Stripe)
        order.Status = OrderStatus.Completed;
        order.CompletedDate = DateTime.Now;

        // Save to database
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
    public List<CartItemRequest> Items { get; set; } = new List<CartItemRequest>();
}

public class CartItemRequest
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
