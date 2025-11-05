using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISW2_Primer_parcial.Data;
using ISW2_Primer_parcial.Models;

namespace ISW2_Primer_parcial.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventarioController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InventarioController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Inventario
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetInventario()
    {
        var inventario = await _context.Inventarios
            .Include(i => i.Producto)
            .Select(i => new
            {
                i.IdProducto,
                Producto = i.Producto!.Nombre,
                i.Existencia,
                MinimoExistencia = i.Producto!.MinimoExistencia,
                Estado = i.Existencia > i.Producto!.MinimoExistencia ? "Normal" :
                         i.Existencia == i.Producto!.MinimoExistencia ? "En Mínimo" : "Bajo Mínimo",
                i.UltimaFechaActualizacion
            })
            .ToListAsync();

        return inventario;
    }

    // GET: api/Inventario/5
    [HttpGet("{idProducto}")]
    public async Task<ActionResult<object>> GetInventarioProducto(int idProducto)
    {
        var inventario = await _context.Inventarios
            .Include(i => i.Producto)
            .FirstOrDefaultAsync(i => i.IdProducto == idProducto);

        if (inventario == null)
        {
            return NotFound(new { mensaje = "No hay inventario para este producto." });
        }

        var resultado = new
        {
            inventario.IdProducto,
            Producto = inventario.Producto!.Nombre,
            inventario.Existencia,
            MinimoExistencia = inventario.Producto!.MinimoExistencia,
            Estado = inventario.Existencia > inventario.Producto!.MinimoExistencia ? "Normal" :
                     inventario.Existencia == inventario.Producto!.MinimoExistencia ? "En Mínimo" : "Bajo Mínimo",
            inventario.UltimaFechaActualizacion
        };

        return resultado;
    }
}
