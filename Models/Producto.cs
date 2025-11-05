using System.ComponentModel.DataAnnotations;

namespace ISW2_Primer_parcial.Models;

public class Producto
{
    [Key]
    public int IdProducto { get; set; }

    [Required]
    public required string Nombre { get; set; }

    public string? Descripcion { get; set; }

    [Required]
    public decimal PrecioVenta { get; set; }

    [Required]
    public int MinimoExistencia { get; set; } = 0;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime UltimaFechaActualizacion { get; set; } = DateTime.Now;
}