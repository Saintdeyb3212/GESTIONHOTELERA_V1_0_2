using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;
using GESTIONHOTELERA_V1_0_2.Data;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using Microsoft.EntityFrameworkCore;

namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;

public sealed class PagoServicio : IPagoServicio
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public PagoServicio(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ResultadoOperacion<Pago>> RegistrarPagoAsync(Pago pago)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var reserva = await context.Reservas.FirstOrDefaultAsync(r => r.IdReserva == pago.IdReserva && r.Activo);
        if (reserva is null)
        {
            return ResultadoOperacion<Pago>.Error("La reserva no existe o está inactiva.");
        }

        if (reserva.EstadoReserva == EstadosReserva.Cancelada)
        {
            return ResultadoOperacion<Pago>.Error("No se pueden registrar pagos en una reserva cancelada.");
        }

        if (pago.Monto <= 0)
        {
            return ResultadoOperacion<Pago>.Error("El monto debe ser mayor que cero.");
        }

        var metodoExiste = await context.MetodosPago.AnyAsync(m => m.IdMetodoPago == pago.IdMetodoPago && m.Activo);
        if (!metodoExiste)
        {
            return ResultadoOperacion<Pago>.Error("Debe seleccionar un método de pago activo.");
        }

        var totalPagado = await context.Pagos
            .Where(p => p.Activo && p.IdReserva == pago.IdReserva)
            .SumAsync(p => p.Monto);

        if (totalPagado + pago.Monto > reserva.TotalEstimado)
        {
            return ResultadoOperacion<Pago>.Error("El pago supera el saldo pendiente de la reserva.");
        }

        pago.FechaPago = pago.FechaPago == default ? DateTime.UtcNow : pago.FechaPago;
        pago.Activo = true;
        pago.FechaRegistro = DateTime.UtcNow;

        await context.Pagos.AddAsync(pago);
        await context.SaveChangesAsync();

        return ResultadoOperacion<Pago>.Ok(pago, "Pago registrado correctamente.");
    }

    public async Task<decimal> ObtenerSaldoPendienteAsync(int idReserva)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var reserva = await context.Reservas.AsNoTracking().FirstOrDefaultAsync(r => r.IdReserva == idReserva && r.Activo);
        if (reserva is null)
        {
            return 0m;
        }

        var totalPagado = await context.Pagos.AsNoTracking()
            .Where(p => p.Activo && p.IdReserva == idReserva)
            .SumAsync(p => p.Monto);

        return reserva.TotalEstimado - totalPagado;
    }
}
