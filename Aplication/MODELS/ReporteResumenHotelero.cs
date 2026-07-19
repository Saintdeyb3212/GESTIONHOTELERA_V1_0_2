namespace GESTIONHOTELERA_V1_0_2.Aplication.MODELS;

public class ReporteResumenHotelero
{
    public decimal GananciasMes { get; set; }
    public int ReservasPendientes { get; set; }
    public int ReservasConfirmadas { get; set; }
    public int ReservasCanceladas { get; set; }
    public int ReservasCompletadas { get; set; }
    public int HabitacionesDisponibles { get; set; }
    public int HabitacionesOcupadas { get; set; }
    public int HabitacionesPendientesLimpieza { get; set; }
}
