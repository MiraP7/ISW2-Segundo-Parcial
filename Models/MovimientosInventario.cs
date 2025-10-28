using System.ComponentModel.DataAnnotations;

namespace ISW2_Primer_parcial.Models;

public class MovimientosInventario
{
    [Key]
    public int IdMovimiento { get; set; }

    [Required]
    public int IdProducto { get; set; }

    [Required]
    public DateTime Fecha { get; set; }

    [Required]
    public int Cantidad { get; set; }

    [Required]
    public int IdTipoMovimiento { get; set; }

    public DateTime UltimaFechaActualizacion { get; set; } = DateTime.Now;

    // Navigation
    public Producto? Producto { get; set; }
    public TipoMovimiento? TipoMovimiento { get; set; }
}