using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;

using Microsoft.EntityFrameworkCore;
namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;

public interface IReporteServicio
{
    Task<ReporteHoteleroDetalle> GenerarReporteAsync(DateTime fechaInicio, DateTime fechaFin);
}
