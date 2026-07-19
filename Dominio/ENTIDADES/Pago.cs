using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GESTIONHOTELERA_V1_0_2.Data.ENTIDADES
{
    [Index(nameof(FechaPago))]
    public class Pago : EntidadBase
    {
        [Key]
        public int IdPago { get; set; }

        [Required]
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Precision(18, 2)]
        [Range(0.01, 999999.99, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El tipo de pago es obligatorio.")]
        [StringLength(30, ErrorMessage = "El tipo de pago no puede tener más de 30 caracteres.")]
        public string TipoPago { get; set; } = TiposPago.Adelanto;

        [StringLength(100, ErrorMessage = "El número de operación no puede tener más de 100 caracteres.")]
        public string? NumeroOperacion { get; set; }

        [StringLength(500, ErrorMessage = "La observación no puede tener más de 500 caracteres.")]
        public string? Observacion { get; set; }

        [Required]
        public int IdReserva { get; set; }

        [ForeignKey(nameof(IdReserva))]
        public Reserva Reserva { get; set; } = null!;

        [Required]
        public int IdMetodoPago { get; set; }

        [ForeignKey(nameof(IdMetodoPago))]
        public MetodoPago MetodoPago { get; set; } = null!;
    }
}
