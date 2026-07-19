using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using GESTIONHOTELERA_V1_0_2.Data;

using Microsoft.EntityFrameworkCore;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;

namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;

public sealed class LimpiezaServicio : ILimpiezaServicio
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public LimpiezaServicio(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ResultadoOperacion<Limpieza>> AsignarLimpiezaAsync(int idHabitacion, string responsable, string? observacion = null)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        if (string.IsNullOrWhiteSpace(responsable))
        {
            return ResultadoOperacion<Limpieza>.Error("Debe indicar un responsable de limpieza.");
        }

        var habitacion = await context.Habitaciones.FirstOrDefaultAsync(h => h.IdHabitacion == idHabitacion && h.Activo);
        if (habitacion is null)
        {
            return ResultadoOperacion<Limpieza>.Error("La habitación no existe o está inactiva.");
        }

        if (habitacion.EstadoHabitacion != EstadosHabitacion.PendienteLimpieza)
        {
            return ResultadoOperacion<Limpieza>.Error("Solo se puede asignar limpieza a habitaciones pendientes de limpieza.");
        }

        var limpiezaAbierta = await context.Limpiezas.AnyAsync(l =>
            l.Activo
            && l.IdHabitacion == idHabitacion
            && l.EstadoLimpieza != EstadosLimpieza.Finalizada
            && l.EstadoLimpieza != EstadosLimpieza.Cancelada);

        if (limpiezaAbierta)
        {
            return ResultadoOperacion<Limpieza>.Error("La habitación ya tiene una limpieza pendiente o en proceso.");
        }

        var limpieza = new Limpieza
        {
            IdHabitacion = idHabitacion,
            ResponsableLimpieza = responsable.Trim(),
            Observacion = observacion,
            EstadoLimpieza = EstadosLimpieza.Pendiente,
            FechaAsignacion = DateTime.UtcNow,
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        await context.Limpiezas.AddAsync(limpieza);
        habitacion.EstadoHabitacion = EstadosHabitacion.EnLimpieza;
        habitacion.FechaModificacion = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return ResultadoOperacion<Limpieza>.Ok(limpieza, "Limpieza asignada correctamente.");
    }

    public async Task<ResultadoOperacion> IniciarLimpiezaAsync(int idLimpieza)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        var limpieza = await context.Limpiezas.FirstOrDefaultAsync(l => l.IdLimpieza == idLimpieza && l.Activo);
        if (limpieza is null)
        {
            return ResultadoOperacion.Error("La limpieza no existe o está inactiva.");
        }

        if (limpieza.EstadoLimpieza != EstadosLimpieza.Pendiente)
        {
            return ResultadoOperacion.Error("Solo se pueden iniciar limpiezas pendientes.");
        }

        limpieza.EstadoLimpieza = EstadosLimpieza.EnProceso;
        limpieza.FechaInicio = DateTime.UtcNow;
        limpieza.FechaModificacion = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return ResultadoOperacion.Ok("Limpieza iniciada correctamente.");
    }

    public async Task<ResultadoOperacion> FinalizarLimpiezaAsync(int idLimpieza, string? observacion = null)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        var limpieza = await context.Limpiezas.FirstOrDefaultAsync(l => l.IdLimpieza == idLimpieza && l.Activo);
        if (limpieza is null)
        {
            return ResultadoOperacion.Error("La limpieza no existe o está inactiva.");
        }

        if (limpieza.EstadoLimpieza is not (EstadosLimpieza.Pendiente or EstadosLimpieza.EnProceso))
        {
            return ResultadoOperacion.Error("Solo se pueden finalizar limpiezas pendientes o en proceso.");
        }

        limpieza.EstadoLimpieza = EstadosLimpieza.Finalizada;
        limpieza.FechaInicio ??= DateTime.UtcNow;
        limpieza.FechaFinalizacion = DateTime.UtcNow;
        limpieza.Observacion = observacion ?? limpieza.Observacion;
        limpieza.FechaModificacion = DateTime.UtcNow;

        var habitacion = await context.Habitaciones.FirstOrDefaultAsync(h => h.IdHabitacion == limpieza.IdHabitacion);
        if (habitacion is not null)
        {
            habitacion.EstadoHabitacion = EstadosHabitacion.Disponible;
            habitacion.FechaModificacion = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return ResultadoOperacion.Ok("Limpieza finalizada. La habitación vuelve a estar disponible.");
    }
}
