using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GESTIONHOTELERA_V1_0_2.Data.ENTIDADES
{
    [Index(nameof(NombreMetodoPago), IsUnique = true)]
    public class MetodoPago : EntidadBase
    {
        [Key]
        public int IdMetodoPago { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        [StringLength(50, ErrorMessage = "El método de pago no puede tener más de 50 caracteres.")]
        public string NombreMetodoPago { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "La descripción no puede tener más de 250 caracteres.")]
        public string? DescripcionMetodoPago { get; set; }

        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}
