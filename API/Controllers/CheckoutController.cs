using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Stripe.Checkout;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IConfiguration _configuration;

    public CheckoutController(DataContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // POST: api/checkout/create-session
    [HttpPost("create-session")]
    public async Task<ActionResult<CreateSessionResponse>> CreateCheckoutSession(
        [FromBody] CreateCheckoutSessionRequest request)
    {
        // Check for empty cart first (more specific error)
        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest("Cart is empty");
        }

        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        // Create line items for Stripe from cart
        var lineItems = new List<SessionLineItemOptions>();
        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            // Verify product exists and get current price from database
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                return BadRequest($"Product with ID {item.ProductId} not found");
            }

            // SECURITY: Always use database price, never trust client
            var priceToUse = product.IsOnSale ? product.SalePrice!.Value : product.Price;

            // Stripe uses cents/pennies, so multiply by 100
            var priceInCents = (long)(priceToUse * 100);

            // Create Stripe line item
            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = product.Name,
                        Description = product.Description,
                    },
                    UnitAmount = priceInCents,
                },
                Quantity = item.Quantity,
            });

            // Create order item for database
            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                PriceAtPurchase = priceToUse
            });

            totalAmount += priceToUse * item.Quantity;
        }

        // Create order in database with Pending status
        var order = new Order
        {
            CustomerEmail = request.CustomerEmail,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount,
            CreatedDate = DateTime.Now,
            OrderItems = orderItems
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Create Stripe Checkout Session
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = $"http://localhost:5173/order/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = "http://localhost:5173/checkout/cancelled",
            CustomerEmail = request.CustomerEmail,
            ClientReferenceId = order.Id.ToString(), // Link session to our order
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        // Save Stripe session ID to order
        order.StripeSessionId = session.Id;
        await _context.SaveChangesAsync();

        // Return session ID and URL to client
        return Ok(new CreateSessionResponse
        {
            SessionId = session.Id,
            Url = session.Url
        });
    }
}

// Request/Response Models
public class CreateCheckoutSessionRequest
{
    [Required]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    public List<CartItemRequest> Items { get; set; } = new List<CartItemRequest>();
}

public class CreateSessionResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
