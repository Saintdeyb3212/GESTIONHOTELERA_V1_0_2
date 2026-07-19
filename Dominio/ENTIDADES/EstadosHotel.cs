namespace GESTIONHOTELERA_V1_0_2.Data.ENTIDADES
{
    public static class EstadosHabitacion
    {
        public const string Disponible = "Disponible";
        public const string Reservada = "Reservada";
        public const string Ocupada = "Ocupada";
        public const string PendienteLimpieza = "Pendiente Limpieza";
        public const string EnLimpieza = "En Limpieza";
        public const string Mantenimiento = "Mantenimiento";
        public const string Inactiva = "Inactiva";
    }

    public static class EstadosReserva
    {
        public const string Pendiente = "Pendiente";
        public const string Confirmada = "Confirmada";
        public const string CheckIn = "Check-In";
        public const string Completada = "Completada";
        public const string Cancelada = "Cancelada";
    }

    public static class EstadosLimpieza
    {
        public const string Pendiente = "Pendiente";
        public const string EnProceso = "En Proceso";
        public const string Finalizada = "Finalizada";
        public const string Cancelada = "Cancelada";
    }

    public static class TiposPago
    {
        public const string Adelanto = "Adelanto";
        public const string PagoFinal = "Pago Final";
    }

}
