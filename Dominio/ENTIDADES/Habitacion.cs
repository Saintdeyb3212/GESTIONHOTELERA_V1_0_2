using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace GESTIONHOTELERA_V1_0_2.Data.ENTIDADES
{
    [Index(nameof(NumeroHabitacion), IsUnique = true)]
    public class Habitacion : EntidadBase
    {
        [Key]
        public int IdHabitacion { get; set; }

        [Required(ErrorMessage = "El número de habitación es obligatorio.")]
        [StringLength(20, ErrorMessage = "El número de habitación no puede tener más de 20 caracteres.")]
        public string NumeroHabitacion { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "El piso no puede tener más de 100 caracteres.")]
        public string? Piso { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres.")]
        public string? DescripcionHabitacion { get; set; }

        [Required(ErrorMessage = "El estado de la habitación es obligatorio.")]
        [StringLength(30, ErrorMessage = "El estado no puede tener más de 30 caracteres.")]
        public string EstadoHabitacion { get; set; } = EstadosHabitacion.Disponible;

        [Required(ErrorMessage = "El tipo de habitación es obligatorio.")]
        public int IdTipoHabitacion { get; set; }

        [ForeignKey(nameof(IdTipoHabitacion))]
        public TipoHabitacion TipoHabitacion { get; set; } = null!;

        public ICollection<DetalleReserva> DetallesReserva { get; set; } = new List<DetalleReserva>();
        public ICollection<Limpieza> Limpiezas { get; set; } = new List<Limpieza>();
    }
}
