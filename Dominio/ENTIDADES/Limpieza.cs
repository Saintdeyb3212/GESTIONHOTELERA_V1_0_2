using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace GESTIONHOTELERA_V1_0_2.Data.ENTIDADES
{
    [Index(nameof(EstadoLimpieza))]
    [Index(nameof(FechaAsignacion))]
    public class Limpieza : EntidadBase
    {
        [Key]
        public int IdLimpieza { get; set; }

        [Required]
        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }

        [Required(ErrorMessage = "El responsable de limpieza es obligatorio.")]
        [StringLength(150, ErrorMessage = "El responsable no puede tener más de 150 caracteres.")]
        public string ResponsableLimpieza { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado de limpieza es obligatorio.")]
        [StringLength(30, ErrorMessage = "El estado no puede tener más de 30 caracteres.")]
        public string EstadoLimpieza { get; set; } = EstadosLimpieza.Pendiente;

        [StringLength(500, ErrorMessage = "La observación no puede tener más de 500 caracteres.")]
        public string? Observacion { get; set; }

        [Required]
        public int IdHabitacion { get; set; }

        [ForeignKey(nameof(IdHabitacion))]
        public Habitacion Habitacion { get; set; } = null!;
    }

}
