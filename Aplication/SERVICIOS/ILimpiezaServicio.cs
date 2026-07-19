using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;

using Microsoft.EntityFrameworkCore;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;

namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;

public interface ILimpiezaServicio
{
    Task<ResultadoOperacion<Limpieza>> AsignarLimpiezaAsync(int idHabitacion, string responsable, string? observacion = null);
    Task<ResultadoOperacion> IniciarLimpiezaAsync(int idLimpieza);
    Task<ResultadoOperacion> FinalizarLimpiezaAsync(int idLimpieza, string? observacion = null);
}
