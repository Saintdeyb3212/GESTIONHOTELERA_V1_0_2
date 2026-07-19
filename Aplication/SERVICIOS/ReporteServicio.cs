using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using GESTIONHOTELERA_V1_0_2.Data;

using Microsoft.EntityFrameworkCore;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;
namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;
public sealed class ReporteServicio : IReporteServicio
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ReporteServicio(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ReporteHoteleroDetalle> GenerarReporteAsync(DateTime fechaInicio, DateTime fechaFin)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        var desde = fechaInicio.Date;
        var hasta = fechaFin.Date;

        if (hasta < desde)
        {
            (desde, hasta) = (hasta, desde);
        }

        var hastaExclusivo = hasta.AddDays(1);

        var pagosPeriodo = context.Pagos.AsNoTracking()
            .Where(p => p.Activo && p.FechaPago >= desde && p.FechaPago < hastaExclusivo);

        var reservasPeriodo = context.Reservas.AsNoTracking()
            .Where(r => r.Activo && r.FechaReserva >= desde && r.FechaReserva < hastaExclusivo);

        var totalReservas = await reservasPeriodo.CountAsync();
        var ganancias = await pagosPeriodo.SumAsync(p => (decimal?)p.Monto) ?? 0m;
        var totalEstimadoReservas = await reservasPeriodo
            .Where(r => r.EstadoReserva != EstadosReserva.Cancelada)
            .SumAsync(r => (decimal?)r.TotalEstimado) ?? 0m;

        var habitacionesActivas = await context.Habitaciones.AsNoTracking()
            .CountAsync(h => h.Activo);

        var habitacionesOcupadas = await ContarHabitacionesAsync(context, EstadosHabitacion.Ocupada);
        var habitacionesReservadas = await ContarHabitacionesAsync(context, EstadosHabitacion.Reservada);

        var reporte = new ReporteHoteleroDetalle
        {
            FechaInicio = desde,
            FechaFin = hasta,
            Ganancias = ganancias,
            TotalEstimadoReservas = totalEstimadoReservas,
            SaldoPendiente = Math.Max(totalEstimadoReservas - ganancias, 0m),
            TicketPromedio = totalReservas == 0 ? 0m : Math.Round(totalEstimadoReservas / totalReservas, 2),
            TotalReservas = totalReservas,
            ReservasPendientes = await ContarReservasAsync(context, EstadosReserva.Pendiente, desde, hastaExclusivo),
            ReservasConfirmadas = await ContarReservasAsync(context, EstadosReserva.Confirmada, desde, hastaExclusivo),
            ReservasCheckIn = await ContarReservasAsync(context, EstadosReserva.CheckIn, desde, hastaExclusivo),
            ReservasCompletadas = await ContarReservasAsync(context, EstadosReserva.Completada, desde, hastaExclusivo),
            ReservasCanceladas = await ContarReservasAsync(context, EstadosReserva.Cancelada, desde, hastaExclusivo),
            TotalHabitacionesActivas = habitacionesActivas,
            HabitacionesDisponibles = await ContarHabitacionesAsync(context, EstadosHabitacion.Disponible),
            HabitacionesReservadas = habitacionesReservadas,
            HabitacionesOcupadas = habitacionesOcupadas,
            HabitacionesPendientesLimpieza = await ContarHabitacionesAsync(context, EstadosHabitacion.PendienteLimpieza),
            HabitacionesEnLimpieza = await ContarHabitacionesAsync(context, EstadosHabitacion.EnLimpieza),
            HabitacionesMantenimiento = await ContarHabitacionesAsync(context, EstadosHabitacion.Mantenimiento),
            LimpiezasPendientes = await ContarLimpiezasAsync(context, EstadosLimpieza.Pendiente, desde, hastaExclusivo),
            LimpiezasEnProceso = await ContarLimpiezasAsync(context, EstadosLimpieza.EnProceso, desde, hastaExclusivo),
            LimpiezasFinalizadas = await ContarLimpiezasAsync(context, EstadosLimpieza.Finalizada, desde, hastaExclusivo),
            IngresosPorMetodoPago = await pagosPeriodo
                .GroupBy(p => p.MetodoPago.NombreMetodoPago)
                .Select(g => new IngresoPorMetodoPago
                {
                    MetodoPago = g.Key,
                    Total = g.Sum(p => p.Monto),
                    CantidadPagos = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync(),
            ReservasPorEstado = await reservasPeriodo
                .GroupBy(r => r.EstadoReserva)
                .Select(g => new ReservaPorEstado
                {
                    Estado = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToListAsync(),
            HabitacionesMasReservadas = await context.DetallesReserva.AsNoTracking()
                .Where(d => d.Activo && d.Reserva.Activo && d.Reserva.FechaReserva >= desde && d.Reserva.FechaReserva < hastaExclusivo)
                .GroupBy(d => new
                {
                    d.Habitacion.NumeroHabitacion,
                    d.Habitacion.TipoHabitacion.NombreTipoHabitacion
                })
                .Select(g => new HabitacionMasReservada
                {
                    NumeroHabitacion = g.Key.NumeroHabitacion,
                    TipoHabitacion = g.Key.NombreTipoHabitacion,
                    CantidadReservas = g.Count(),
                    TotalGenerado = g.Sum(d => d.SubTotal)
                })
                .OrderByDescending(x => x.CantidadReservas)
                .ThenByDescending(x => x.TotalGenerado)
                .Take(10)
                .ToListAsync(),
            ClientesFrecuentes = await reservasPeriodo
                .GroupBy(r => new
                {
                    r.Cliente.Nombres,
                    r.Cliente.Apellidos,
                    r.Cliente.NumeroDocumento
                })
                .Select(g => new ClienteFrecuente
                {
                    Cliente = g.Key.Apellidos + ", " + g.Key.Nombres,
                    Documento = g.Key.NumeroDocumento,
                    CantidadReservas = g.Count(),
                    TotalReservado = g.Where(r => r.EstadoReserva != EstadosReserva.Cancelada).Sum(r => r.TotalEstimado)
                })
                .OrderByDescending(x => x.CantidadReservas)
                .ThenByDescending(x => x.TotalReservado)
                .Take(10)
                .ToListAsync(),
            IngresosDiarios = await pagosPeriodo
                .GroupBy(p => p.FechaPago.Date)
                .Select(g => new IngresoDiario
                {
                    Fecha = g.Key,
                    Total = g.Sum(p => p.Monto),
                    CantidadPagos = g.Count()
                })
                .OrderBy(x => x.Fecha)
                .ToListAsync(),
            OcupacionPorTipoHabitacion = await context.DetallesReserva.AsNoTracking()
                .Where(d => d.Activo && d.Reserva.Activo && d.Reserva.EstadoReserva != EstadosReserva.Cancelada && d.Reserva.FechaReserva >= desde && d.Reserva.FechaReserva < hastaExclusivo)
                .GroupBy(d => d.Habitacion.TipoHabitacion.NombreTipoHabitacion)
                .Select(g => new OcupacionPorTipoHabitacion
                {
                    TipoHabitacion = g.Key,
                    CantidadReservas = g.Count(),
                    TotalGenerado = g.Sum(d => d.SubTotal)
                })
                .OrderByDescending(x => x.CantidadReservas)
                .ToListAsync()
        };

        reporte.PorcentajeCancelacion = CalcularPorcentaje(reporte.ReservasCanceladas, reporte.TotalReservas);
        reporte.PorcentajeCompletadas = CalcularPorcentaje(reporte.ReservasCompletadas, reporte.TotalReservas);
        reporte.PorcentajeOcupacionActual = CalcularPorcentaje(habitacionesOcupadas + habitacionesReservadas, habitacionesActivas);

        return reporte;
    }

    private static Task<int> ContarReservasAsync(ApplicationDbContext context, string estado, DateTime desde, DateTime hastaExclusivo)
    {
        return context.Reservas.AsNoTracking()
            .CountAsync(r => r.Activo && r.EstadoReserva == estado && r.FechaReserva >= desde && r.FechaReserva < hastaExclusivo);
    }

    private static Task<int> ContarHabitacionesAsync(ApplicationDbContext context, string estado)
    {
        return context.Habitaciones.AsNoTracking()
            .CountAsync(h => h.Activo && h.EstadoHabitacion == estado);
    }

    private static Task<int> ContarLimpiezasAsync(ApplicationDbContext context, string estado, DateTime desde, DateTime hastaExclusivo)
    {
        return context.Limpiezas.AsNoTracking()
            .CountAsync(l => l.Activo && l.EstadoLimpieza == estado && l.FechaAsignacion >= desde && l.FechaAsignacion < hastaExclusivo);
    }

    private static decimal CalcularPorcentaje(int cantidad, int total)
    {
        return total <= 0 ? 0m : Math.Round(cantidad * 100m / total, 2);
    }
}
