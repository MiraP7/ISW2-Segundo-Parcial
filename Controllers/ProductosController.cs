using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ISW2_Primer_parcial.Data;
using ISW2_Primer_parcial.Models;
using System.Data;

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
        var productos = new List<Producto>();
        
        using (var connection = _context.Database.GetDbConnection())
        {
            await connection.OpenAsync();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "EXEC GetProductos";
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        productos.Add(new Producto
                        {
                            IdProducto = reader.GetInt32(reader.GetOrdinal("IdProducto")),
                            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                            CodigoProducto = reader.GetString(reader.GetOrdinal("CodigoProducto")),
                            Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
                            PrecioVenta = reader.GetDecimal(reader.GetOrdinal("PrecioVenta")),
                            MinimoExistencia = reader.GetInt32(reader.GetOrdinal("MinimoExistencia")),
                            Eliminado = reader.GetBoolean(reader.GetOrdinal("Eliminado")),
                            FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                            UltimaFechaActualizacion = reader.GetDateTime(reader.GetOrdinal("UltimaFechaActualizacion"))
                        });
                    }
                }
            }
        }
        
        return productos;
    }

    // GET: api/Productos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> GetProducto(int id)
    {
        Producto? producto = null;
        
        using (var connection = _context.Database.GetDbConnection())
        {
            await connection.OpenAsync();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "EXEC GetProducto @IdProducto";
                command.Parameters.Add(new SqlParameter("@IdProducto", id));
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        producto = new Producto
                        {
                            IdProducto = reader.GetInt32(reader.GetOrdinal("IdProducto")),
                            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                            CodigoProducto = reader.GetString(reader.GetOrdinal("CodigoProducto")),
                            Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
                            PrecioVenta = reader.GetDecimal(reader.GetOrdinal("PrecioVenta")),
                            MinimoExistencia = reader.GetInt32(reader.GetOrdinal("MinimoExistencia")),
                            Eliminado = reader.GetBoolean(reader.GetOrdinal("Eliminado")),
                            FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                            UltimaFechaActualizacion = reader.GetDateTime(reader.GetOrdinal("UltimaFechaActualizacion"))
                        };
                    }
                }
            }
        }
        
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
        int nuevoId = 0;
        string? codigoGenerado = null;
        string? mensaje = null;

        using (var connection = (SqlConnection)_context.Database.GetDbConnection())
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand("InsertProducto", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@Nombre", producto.Nombre));
                command.Parameters.Add(new SqlParameter("@Descripcion", (object?)producto.Descripcion ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@PrecioVenta", producto.PrecioVenta));
                command.Parameters.Add(new SqlParameter("@MinimoExistencia", producto.MinimoExistencia));
                
                var idParam = new SqlParameter("@IdProducto", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var codigoParam = new SqlParameter("@CodigoProducto", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                var mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                
                command.Parameters.Add(idParam);
                command.Parameters.Add(codigoParam);
                command.Parameters.Add(mensajeParam);

                await command.ExecuteNonQueryAsync();

                nuevoId = (int)idParam.Value;
                codigoGenerado = codigoParam.Value?.ToString();
                mensaje = mensajeParam.Value?.ToString();
            }
        }

        if (nuevoId == -1)
        {
            return BadRequest(mensaje);
        }

        producto.IdProducto = nuevoId;
        producto.CodigoProducto = codigoGenerado ?? "";
        return CreatedAtAction(nameof(GetProducto), new { id = nuevoId }, producto);
    }

    // PUT: api/Productos/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProducto(int id, Producto producto)
    {
        if (id != producto.IdProducto)
        {
            return BadRequest("El ID de la URL no coincide con el ID del producto.");
        }

        string? mensaje = null;

        using (var connection = (SqlConnection)_context.Database.GetDbConnection())
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand("UpdateProducto", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@IdProducto", id));
                command.Parameters.Add(new SqlParameter("@Nombre", producto.Nombre));
                command.Parameters.Add(new SqlParameter("@Descripcion", (object?)producto.Descripcion ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@PrecioVenta", producto.PrecioVenta));
                command.Parameters.Add(new SqlParameter("@MinimoExistencia", producto.MinimoExistencia));
                
                var mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                command.Parameters.Add(mensajeParam);

                await command.ExecuteNonQueryAsync();

                mensaje = mensajeParam.Value?.ToString();
            }
        }

        if (mensaje?.Contains("no encontrado") == true)
        {
            return NotFound(mensaje);
        }

        return NoContent();
    }

    // DELETE: api/Productos/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        string? mensaje = null;

        using (var connection = (SqlConnection)_context.Database.GetDbConnection())
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand("DeleteProducto", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@IdProducto", id));
                
                var mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                command.Parameters.Add(mensajeParam);

                await command.ExecuteNonQueryAsync();

                mensaje = mensajeParam.Value?.ToString();
            }
        }

        if (mensaje?.Contains("no encontrado") == true)
        {
            return NotFound(mensaje);
        }

        return NoContent();
    }
}