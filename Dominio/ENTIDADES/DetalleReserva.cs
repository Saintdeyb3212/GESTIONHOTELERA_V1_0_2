using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace GESTIONHOTELERA_V1_0_2.Data.ENTIDADES
{
    [Index(nameof(IdReserva), nameof(IdHabitacion), IsUnique = true)]
    public class DetalleReserva : EntidadBase
    {
        [Key]
        public int IdDetalleReserva { get; set; }

        [Required]
        public int IdReserva { get; set; }

        [ForeignKey(nameof(IdReserva))]
        public Reserva Reserva { get; set; } = null!;

        [Required]
        public int IdHabitacion { get; set; }

        [ForeignKey(nameof(IdHabitacion))]
        public Habitacion Habitacion { get; set; } = null!;

        [Required]
        [Precision(18, 2)]
        [Range(0.01, 999999.99, ErrorMessage = "El precio por noche debe ser mayor que cero.")]
        public decimal PrecioPorNoche { get; set; }

        [Required]
        [Range(1, 365, ErrorMessage = "La cantidad de noches debe estar entre 1 y 365.")]
        public int CantidadNoches { get; set; }

        [Required]
        [Precision(18, 2)]
        [Range(0.01, 999999.99, ErrorMessage = "El subtotal debe ser mayor que cero.")]
        public decimal SubTotal { get; set; }
    }
}
