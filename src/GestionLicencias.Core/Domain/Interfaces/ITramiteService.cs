using GestionLicencias.Core.Domain.Entities;

namespace GestionLicencias.Core.Domain.Interfaces;

public interface ITramiteService<T> where T : BaseTramite
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task<T?> ObtenerPorRutAsync(string rut);
    Task<IEnumerable<T>> ObtenerTodosAsync(int pagina, int pageSize);
    Task<(IEnumerable<T> Resultados, int Total)> BuscarAsync(BusquedaFilter filter);
    Task<T> CrearAsync(T tramite);
    Task<T> ActualizarAsync(T tramite);
    Task<bool> EliminarAsync(int id);
    Task<T> CambiarEstadoAsync(int id, string estado, string usuarioId, string? ip = null);
    (bool IsValid, string Message) ValidarTramite(T tramite);
    Task<int> ObtenerTotalAsync();
    Task<Dictionary<string, int>> ObtenerEstadisticasPorEstadoAsync();
}

public class BusquedaFilter
{
    public int Pagina { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Termino { get; set; }
    public string? Rut { get; set; }
    public string? Nombre { get; set; }
    public string? Estado { get; set; }
    public string? EstadoCarpeta { get; set; }
    public string? Otorgamiento { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool SoloActivos { get; set; } = true;
    public string OrdenarPor { get; set; } = "FechaIngreso";
    public string Direccion { get; set; } = "desc";
}
