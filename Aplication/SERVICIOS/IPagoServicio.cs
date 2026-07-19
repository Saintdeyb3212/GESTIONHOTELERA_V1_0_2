using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using Microsoft.EntityFrameworkCore;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;

namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;

public interface IPagoServicio
{
    Task<ResultadoOperacion<Pago>> RegistrarPagoAsync(Pago pago);
    Task<decimal> ObtenerSaldoPendienteAsync(int idReserva);
}
