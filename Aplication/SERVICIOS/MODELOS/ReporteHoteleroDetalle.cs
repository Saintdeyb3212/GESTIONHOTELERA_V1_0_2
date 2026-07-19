namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;

public sealed class ReporteHoteleroDetalle
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public decimal Ganancias { get; set; }
    public decimal TotalEstimadoReservas { get; set; }
    public decimal SaldoPendiente { get; set; }
    public decimal TicketPromedio { get; set; }

    public int TotalReservas { get; set; }
    public int ReservasPendientes { get; set; }
    public int ReservasConfirmadas { get; set; }
    public int ReservasCheckIn { get; set; }
    public int ReservasCompletadas { get; set; }
    public int ReservasCanceladas { get; set; }
    public decimal PorcentajeCancelacion { get; set; }
    public decimal PorcentajeCompletadas { get; set; }

    public int TotalHabitacionesActivas { get; set; }
    public int HabitacionesDisponibles { get; set; }
    public int HabitacionesReservadas { get; set; }
    public int HabitacionesOcupadas { get; set; }
    public int HabitacionesPendientesLimpieza { get; set; }
    public int HabitacionesEnLimpieza { get; set; }
    public int HabitacionesMantenimiento { get; set; }
    public decimal PorcentajeOcupacionActual { get; set; }

    public int LimpiezasPendientes { get; set; }
    public int LimpiezasEnProceso { get; set; }
    public int LimpiezasFinalizadas { get; set; }

    public List<IngresoPorMetodoPago> IngresosPorMetodoPago { get; set; } = new List<IngresoPorMetodoPago>();
    public List<ReservaPorEstado> ReservasPorEstado { get; set; } = new List<ReservaPorEstado>();
    public List<HabitacionMasReservada> HabitacionesMasReservadas { get; set; } = new List<HabitacionMasReservada>();
    public List<ClienteFrecuente> ClientesFrecuentes { get; set; } = new List<ClienteFrecuente>();
    public List<IngresoDiario> IngresosDiarios { get; set; } = new List<IngresoDiario>();
    public List<OcupacionPorTipoHabitacion> OcupacionPorTipoHabitacion { get; set; } = new List<OcupacionPorTipoHabitacion>();
}

public sealed class IngresoPorMetodoPago
{
    public string MetodoPago { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int CantidadPagos { get; set; }
}

public sealed class ReservaPorEstado
{
    public string Estado { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public sealed class HabitacionMasReservada
{
    public string NumeroHabitacion { get; set; } = string.Empty;
    public string TipoHabitacion { get; set; } = string.Empty;
    public int CantidadReservas { get; set; }
    public decimal TotalGenerado { get; set; }
}

public sealed class ClienteFrecuente
{
    public string Cliente { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public int CantidadReservas { get; set; }
    public decimal TotalReservado { get; set; }
}

public sealed class IngresoDiario
{
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public int CantidadPagos { get; set; }
}

public sealed class OcupacionPorTipoHabitacion
{
    public string TipoHabitacion { get; set; } = string.Empty;
    public int CantidadReservas { get; set; }
    public decimal TotalGenerado { get; set; }
}
