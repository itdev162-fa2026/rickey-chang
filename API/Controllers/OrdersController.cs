using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
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
