namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;

public sealed class ResultadoOperacion
{
    private ResultadoOperacion(bool exitoso, string mensaje)
    {
        Exitoso = exitoso;
        Mensaje = mensaje;
    }

    public bool Exitoso { get; }
    public string Mensaje { get; }

    public static ResultadoOperacion Ok(string mensaje = "Operación realizada correctamente.") => new(true, mensaje);
    public static ResultadoOperacion Error(string mensaje) => new(false, mensaje);
}

public sealed class ResultadoOperacion<T>
{
    private ResultadoOperacion(bool exitoso, string mensaje, T? valor)
    {
        Exitoso = exitoso;
        Mensaje = mensaje;
        Valor = valor;
    }

    public bool Exitoso { get; }
    public string Mensaje { get; }
    public T? Valor { get; }

    public static ResultadoOperacion<T> Ok(T valor, string mensaje = "Operación realizada correctamente.") => new(true, mensaje, valor);
    public static ResultadoOperacion<T> Error(string mensaje) => new(false, mensaje, default);
}
