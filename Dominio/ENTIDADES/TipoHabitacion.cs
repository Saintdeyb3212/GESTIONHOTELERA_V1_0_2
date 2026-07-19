using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GESTIONHOTELERA_V1_0_2.Data.ENTIDADES
{
    [Index(nameof(NombreTipoHabitacion), IsUnique = true)]
    public class TipoHabitacion : EntidadBase
    {
        [Key]
        public int IdTipoHabitacion { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de habitación es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres.")]
        public string NombreTipoHabitacion { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres.")]
        public string? DescripcionTipoHabitacion { get; set; }

        [Required(ErrorMessage = "La capacidad es obligatoria.")]
        [Range(1, 20, ErrorMessage = "La capacidad debe estar entre 1 y 20 personas.")]
        public int CapacidadPersonas { get; set; }

        [Required(ErrorMessage = "El precio por noche es obligatorio.")]
        [Precision(18, 2)]
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal PrecioPorNoche { get; set; }

        public ICollection<Habitacion> Habitaciones { get; set; } = new List<Habitacion>();
    }
}
