using Microsoft.AspNetCore.Mvc;
using TransactionsService.Clients;
using TransactionsService.Data;
using TransactionsService.DTOs;
using TransactionsService.Entities;
using TransactionsService.Models;
using Microsoft.EntityFrameworkCore;

namespace TransactionsService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController(ApplicationDbContext db, IProductsClient products) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<object>> Get([FromQuery] Guid? productId, [FromQuery] TransactionType? type,
            [FromQuery] DateTime? from, [FromQuery] DateTime? to,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 10;

            var query = db.Transactions.AsNoTracking().AsQueryable();

            if (productId.HasValue) query = query.Where(t => t.ProductId == productId.Value);
            if (type.HasValue) query = query.Where(t => t.Type == type.Value);
            if (from.HasValue) query = query.Where(t => t.Date >= from.Value);
            if (to.HasValue) query = query.Where(t => t.Date < to.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransactionDto(t.Id, t.Date, t.Type, t.ProductId, t.Quantity, t.UnitPrice, t.Total, t.Detail))
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TransactionDto>> GetById(Guid id)
        {
            var t = await db.Transactions.FindAsync(id);
            if (t is null) return NotFound();
            return new TransactionDto(t.Id, t.Date, t.Type, t.ProductId, t.Quantity, t.UnitPrice, t.Total, t.Detail);
        }

        [HttpPost]
        public async Task<ActionResult<TransactionDto>> Create([FromBody] CreateTransactionDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // 1) Validar que el producto exista
            var product = await products.GetByIdAsync(dto.ProductId, ct);
            if (product is null) return BadRequest(new { message = "El producto no existe." });

            // 2) Regla de negocio: venta no puede superar stock
            if (dto.Type == TransactionType.Sale && product.Stock < dto.Quantity)
                return BadRequest(new { message = "No se puede vender más del stock disponible." });

            // 3) Ajustar stock primero en Products.Api
            var delta = dto.Type == TransactionType.Purchase ? dto.Quantity : -dto.Quantity;
            var adjusted = await products.AdjustStockAsync(dto.ProductId, delta, dto.Type.ToString(), ct);
            if (adjusted is null)
                return BadRequest(new { message = "No fue posible ajustar el stock. Intenta nuevamente." });

            // 4) Registrar transacción
            var entity = new InventoryTransaction
            {
                Type = dto.Type,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                Detail = dto.Detail
            };
            db.Transactions.Add(entity);
            await db.SaveChangesAsync(ct);

            var result = new TransactionDto(entity.Id, entity.Date, entity.Type, entity.ProductId, entity.Quantity, entity.UnitPrice, entity.Total, entity.Detail);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TransactionDto>> Update(Guid id, [FromBody] UpdateTransactionDto dto, CancellationToken ct)
        {
            var entity = await db.Transactions.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (entity is null) return NotFound();

            // Nota: para simplificar, actualizar no re-ajusta stock.
            entity.Type = dto.Type;
            entity.Quantity = dto.Quantity;
            entity.UnitPrice = dto.UnitPrice;
            entity.Detail = dto.Detail;

            await db.SaveChangesAsync(ct);

            var result = new TransactionDto(entity.Id, entity.Date, entity.Type, entity.ProductId, entity.Quantity, entity.UnitPrice, entity.Total, entity.Detail);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var entity = await db.Transactions.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (entity is null) return NotFound();

            // Nota: eliminar no re-ajusta stock en este esqueleto.
            db.Transactions.Remove(entity);
            await db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
