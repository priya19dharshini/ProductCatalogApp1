using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalogApp.Data;
using ProductCatalogApp.Models;

namespace ProductCatalogApp.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET api/products
    // GET api/products?category=Books
    // GET api/products?search=pen
    // GET api/products?category=Stationery&search=pen
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll(
        [FromQuery] string? category,
        [FromQuery] string? search)
    {
        IQueryable<Product> query = _context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category.Trim();
            query = query.Where(p => p.Category == cat);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Name.Contains(term) ||
                                     (p.Description != null && p.Description.Contains(term)));
        }

        var products = await query.OrderBy(p => p.Id).ToListAsync();
        return Ok(products);   // empty list [] when nothing matches
    }

    // GET api/products/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
            return NotFound(new { message = $"Product with ID {id} was not found." });

        return Ok(product);
    }

    // POST api/products
    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product product)
    {
        product.Id = 0;   // ignore any Id sent by the client; the database generates it

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT api/products/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Product>> Update(int id, Product input)
    {
        var existing = await _context.Products.FindAsync(id);

        if (existing is null)
            return NotFound(new { message = $"Product with ID {id} was not found." });

        existing.Name = input.Name;
        existing.Description = input.Description;
        existing.Category = input.Category;
        existing.Price = input.Price;
        existing.StockQuantity = input.StockQuantity;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    // DELETE api/products/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _context.Products.FindAsync(id);

        if (existing is null)
            return NotFound(new { message = $"Product with ID {id} was not found." });

        _context.Products.Remove(existing);
        await _context.SaveChangesAsync();

        return NoContent();   // 204
    }
}