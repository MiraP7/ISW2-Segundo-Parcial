using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISW2_Primer_parcial.Data;
using ISW2_Primer_parcial.Models;

namespace ISW2_Primer_parcial.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Productos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
    {
        return await _context.Productos.FromSqlRaw("EXEC GetProductos").ToListAsync();
    }

    // GET: api/Productos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> GetProducto(int id)
    {
        var producto = await _context.Productos.FromSqlRaw("EXEC GetProducto @p0", id).FirstOrDefaultAsync();

        if (producto == null)
        {
            return NotFound();
        }

        return producto;
    }

    // POST: api/Productos
    [HttpPost]
    public async Task<ActionResult<Producto>> PostProducto(Producto producto)
    {
        var id = await _context.Productos.FromSqlRaw("EXEC InsertProducto @p0, @p1, @p2", producto.Nombre, producto.Descripcion ?? "", producto.PrecioVenta).Select(p => p.IdProducto).FirstOrDefaultAsync();
        producto.IdProducto = id;
        return CreatedAtAction(nameof(GetProducto), new { id = producto.IdProducto }, producto);
    }

    // PUT: api/Productos/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProducto(int id, Producto producto)
    {
        if (id != producto.IdProducto)
        {
            return BadRequest();
        }

        await _context.Database.ExecuteSqlRawAsync("EXEC UpdateProducto @p0, @p1, @p2, @p3", id, producto.Nombre, producto.Descripcion ?? "", producto.PrecioVenta);
        return NoContent();
    }

    // DELETE: api/Productos/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        await _context.Database.ExecuteSqlRawAsync("EXEC DeleteProducto @p0", id);
        return NoContent();
    }
}