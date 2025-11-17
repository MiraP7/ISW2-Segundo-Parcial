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
    private const string CodigoDuplicadoMensaje = "No se puede ingresar este producto porque ya existe ese mismo código de producto en otro producto.";

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
        var codigoEnUso = await _context.Productos
            .IgnoreQueryFilters()
            .AnyAsync(p => p.CodigoProducto == producto.CodigoProducto);

        if (codigoEnUso)
        {
            return BadRequest(CodigoDuplicadoMensaje);
        }

        producto.Eliminado = false;
        producto.FechaCreacion = DateTime.UtcNow;
        producto.UltimaFechaActualizacion = DateTime.UtcNow;

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

        var productoExistente = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);
        if (productoExistente is null)
        {
            return NotFound();
        }

        var codigoEnUso = await _context.Productos
            .IgnoreQueryFilters()
            .AnyAsync(p => p.IdProducto != id && p.CodigoProducto == producto.CodigoProducto);

        if (codigoEnUso)
        {
            return BadRequest(CodigoDuplicadoMensaje);
        }

        productoExistente.Nombre = producto.Nombre;
        productoExistente.Descripcion = producto.Descripcion;
        productoExistente.PrecioVenta = producto.PrecioVenta;
        productoExistente.MinimoExistencia = producto.MinimoExistencia;
        productoExistente.CodigoProducto = producto.CodigoProducto;
        productoExistente.UltimaFechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    // DELETE: api/Productos/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        var producto = await _context.Productos
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.IdProducto == id);

        if (producto == null)
        {
            return NotFound();
        }

        if (producto.Eliminado)
        {
            return NoContent();
        }

        producto.Eliminado = true;
        producto.UltimaFechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}