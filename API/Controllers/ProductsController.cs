using Domain;
using Microsoft.AspNetCore.Mvc;
using Persistence;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    private readonly DataContext _context;

    public ProductsController(ILogger<ProductsController> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetProducts()
    {
        var products = _context.Products.ToList();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetProduct(int id)
    {
        var product = _context.Products.Find(id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }
    [HttpPost]
    public ActionResult<Product> CreateProduct(Product product)
    {
        // Check model validation
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        // Set audit dates
        product.CreatedDate = DateTime.Now;
        product.LastUpdatedDate = DateTime.Now;

        _context.Products.Add(product);
        var success = _context.SaveChanges() > 0;

        if (success)
        {
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        return BadRequest("Failed to create product");
    }

    [HttpPut("{id}")]
    public ActionResult<Product> UpdateProduct(int id, Product product)
    {
        // Check model Validation
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        var existingProduct = _context.Products.Find(id);

        if (existingProduct == null)
        {
            return NotFound();
        }

        // Update all editable properties
        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.IsOnSale = product.IsOnSale;
        existingProduct.SalePrice = product.SalePrice;
        existingProduct.CurrentStock = product.CurrentStock;
        existingProduct.ImageUrl = product.ImageUrl;

        // Update audit date (preserve CreatedDate)
        existingProduct.LastUpdatedDate = DateTime.Now;

        var success = _context.SaveChanges() > 0;

        if (success)
        {
            return Ok(existingProduct);
        }

        return BadRequest("Failed to update product");
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteProduct(int id)
    {
        var product = _context.Products.Find(id);

        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        var success = _context.SaveChanges() > 0;

        if (success)
        {
            return NoContent();
        }

        return BadRequest("Failed to delete product");
    }

    [HttpGet("search")]
    public ActionResult<IEnumerable<Product>> SearchProducts(
    [FromQuery] string? name = null,
    [FromQuery] decimal? minPrice = null,
    [FromQuery] decimal? maxPrice = null,
    [FromQuery] bool? isOnSale = null,
    [FromQuery] bool? inStock = null,
    [FromQuery] string sortBy = "name",
    [FromQuery] string sortOrder = "asc")
    {
        var query = _context.Products.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (isOnSale.HasValue)
        {
            query = query.Where(p => p.IsOnSale == isOnSale.Value);
        }

        if (inStock.HasValue && inStock.Value)
        {
            query = query.Where(p => p.CurrentStock > 0);
        }

        // Execute the query first, then sort in memory for SQLite compatibility
        var products = query.ToList();

        // Apply sorting in memory
        products = sortBy.ToLower() switch
        {
            "price" => sortOrder.ToLower() == "desc"
                ? products.OrderByDescending(p => p.Price).ToList()
                : products.OrderBy(p => p.Price).ToList(),
            "created" => sortOrder.ToLower() == "desc"
                ? products.OrderByDescending(p => p.CreatedDate).ToList()
                : products.OrderBy(p => p.CreatedDate).ToList(),
            "stock" => sortOrder.ToLower() == "desc"
                ? products.OrderByDescending(p => p.CurrentStock).ToList()
                : products.OrderBy(p => p.CurrentStock).ToList(),
            _ => sortOrder.ToLower() == "desc"
                ? products.OrderByDescending(p => p.Name).ToList()
                : products.OrderBy(p => p.Name).ToList()
        };

        return Ok(products);
    }

}
