

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;
namespace ProductService.Controllers;

[ApiController]
[Route("api/productos")]
//Devuelve todos los productos
    public class ProductsController(ApplicationDbContext db):ControllerBase
    {
    [HttpGet]
    public async Task<ActionResult<object>> Get([FromQuery] string? search, [FromQuery] string? category,
        [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 200) pageSize = 10;

        var query = db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => p.Name.Contains(s) || (p.Description != null && p.Description.Contains(s)));
        }
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);
        if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Category, p.ImageUrl, p.Price, p.Stock))
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }
    //Obtiene productos por id
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();
        return new ProductDto(p.Id, p.Name, p.Description, p.Category, p.ImageUrl, p.Price, p.Stock);
    }
    //Crea o inserta un nuevo producto
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductCreateDto dto)
    {
        var entity = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            ImageUrl = dto.ImageUrl,
            Price = dto.Price,
            Stock = dto.Stock
        };
        db.Products.Add(entity);
        await db.SaveChangesAsync();

        var result = new ProductDto(entity.Id, entity.Name, entity.Description, entity.Category, entity.ImageUrl, entity.Price, entity.Stock);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }
    //Actualiza el producto
    [HttpPut("{id:guid}")]
    
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] ProductUpdateDto dto)
    {
        var entity = await db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null) return NotFound();

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Category = dto.Category;
        entity.ImageUrl = dto.ImageUrl;
        entity.Price = dto.Price;
        entity.Stock = dto.Stock;
        entity.UpdatedAt = DateTime.UtcNow;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Conflicto de concurrencia al actualizar el producto." });
        }

        return new ProductDto(entity.Id, entity.Name, entity.Description, entity.Category, entity.ImageUrl, entity.Price, entity.Stock);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await db.Products.FindAsync(id);
        if (entity is null) return NotFound();
        db.Products.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Endpoint para ajustar stock (usado por Transactions.Api).
    [HttpPut("{id:guid}/adjust-stock")]
    public async Task<ActionResult<ProductDto>> AdjustStock(Guid id, [FromBody] AdjustStockRequest req)
    {
        if (req is null) return BadRequest();

        var entity = await db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null) return NotFound(new { message = "Producto no encontrado" });

        var newStock = entity.Stock + req.Delta;
        if (newStock < 0)
            return BadRequest(new { message = "Stock insuficiente para realizar la operación." });

        entity.Stock = newStock;
        entity.UpdatedAt = DateTime.UtcNow;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Conflicto de concurrencia al ajustar stock." });
        }

        return Ok(new ProductDto(entity.Id, entity.Name, entity.Description, entity.Category, entity.ImageUrl, entity.Price, entity.Stock));
    }

}

