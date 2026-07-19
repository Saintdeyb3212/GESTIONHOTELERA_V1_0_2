using System.ComponentModel.DataAnnotations;

namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;

public sealed class ActualizarReservaRequest
{
    [Required]
    public int IdReserva { get; set; }

    [Required]
    public DateTime FechaCheckInProgramada { get; set; } = DateTime.Today;

    [Required]
    public DateTime FechaCheckOutProgramada { get; set; } = DateTime.Today.AddDays(1);

    [Required]
    [Range(1, 100)]
    public int CantidadHuespedes { get; set; } = 1;

    [StringLength(500)]
    public string? Observaciones { get; set; }

    [MinLength(1, ErrorMessage = "Debe seleccionar al menos una habitación.")]
    public List<int> HabitacionesSeleccionadas { get; set; } = new List<int>();
}
