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
        return await _context.Productos.ToListAsync();
    }

    // GET: api/Productos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> GetProducto(int id)
    {
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);

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
        // Insertar directamente en la tabla
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        
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

        _context.Entry(producto).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    // DELETE: api/Productos/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}