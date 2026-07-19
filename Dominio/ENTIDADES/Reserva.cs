using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace GESTIONHOTELERA_V1_0_2.Data.ENTIDADES
{
    [Index(nameof(EstadoReserva))]
    [Index(nameof(FechaCheckInProgramada), nameof(FechaCheckOutProgramada))]
    public class Reserva : EntidadBase
    {
        [Key]
        public int IdReserva { get; set; }

        [Required]
        public DateTime FechaReserva { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "La fecha de check-in programada es obligatoria.")]
        public DateTime FechaCheckInProgramada { get; set; }

        [Required(ErrorMessage = "La fecha de check-out programada es obligatoria.")]
        public DateTime FechaCheckOutProgramada { get; set; }

        public DateTime? FechaCheckInReal { get; set; }
        public DateTime? FechaCheckOutReal { get; set; }

        [Required(ErrorMessage = "La cantidad de huéspedes es obligatoria.")]
        [Range(1, 100, ErrorMessage = "La cantidad de huéspedes debe ser mayor que cero.")]
        public int CantidadHuespedes { get; set; }

        [Required(ErrorMessage = "El estado de la reserva es obligatorio.")]
        [StringLength(30, ErrorMessage = "El estado no puede tener más de 30 caracteres.")]
        public string EstadoReserva { get; set; } = EstadosReserva.Pendiente;

        [Precision(18, 2)]
        [Range(0, 999999.99, ErrorMessage = "El total estimado no puede ser negativo.")]
        public decimal TotalEstimado { get; set; }

        [StringLength(500, ErrorMessage = "La observación no puede tener más de 500 caracteres.")]
        public string? Observaciones { get; set; }

        [StringLength(500, ErrorMessage = "El motivo de cancelación no puede tener más de 500 caracteres.")]
        public string? MotivoCancelacion { get; set; }

        public DateTime? FechaCancelacion { get; set; }

        [Required(ErrorMessage = "El cliente es obligatorio.")]
        public int IdCliente { get; set; }

        [ForeignKey(nameof(IdCliente))]
        public Cliente Cliente { get; set; } = null!;

        public ICollection<DetalleReserva> DetallesReserva { get; set; } = new List<DetalleReserva>();
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}
