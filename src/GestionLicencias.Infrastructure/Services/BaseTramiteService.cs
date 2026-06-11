using GestionLicencias.Core.Domain.Entities;
using GestionLicencias.Core.Domain.Interfaces;
using GestionLicencias.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionLicencias.Infrastructure.Services;

public class BaseTramiteService<T> : ITramiteService<T> where T : BaseTramite
{
    protected readonly GestionLicenciasDbContext Context;
    protected readonly ILogger Logger;

    public BaseTramiteService(GestionLicenciasDbContext context, ILogger logger)
    {
        Context = context;
        Logger = logger;
    }

    public virtual async Task<T?> ObtenerPorIdAsync(int id) =>
        await Context.Set<T>().FirstOrDefaultAsync(t => t.Id == id);

    public virtual async Task<T?> ObtenerPorRutAsync(string rut) =>
        await Context.Set<T>().FirstOrDefaultAsync(t => t.RUT == rut && t.Activo);

    public virtual async Task<IEnumerable<T>> ObtenerTodosAsync(int pagina, int pageSize) =>
        await Context.Set<T>()
            .Where(t => t.Activo)
            .OrderByDescending(t => t.FechaIngreso)
            .Skip((pagina - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public virtual async Task<(IEnumerable<T> Resultados, int Total)> BuscarAsync(BusquedaFilter filter)
    {
        var query = AplicarFiltros(Context.Set<T>().AsQueryable(), filter);
        var total = await query.CountAsync();
        var resultados = await query
            .OrderByDescending(t => t.FechaIngreso)
            .Skip((filter.Pagina - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
        return (resultados, total);
    }

    protected virtual IQueryable<T> AplicarFiltros(IQueryable<T> query, BusquedaFilter filter)
    {
        if (filter.SoloActivos)
            query = query.Where(t => t.Activo);
        if (!string.IsNullOrWhiteSpace(filter.Termino))
            query = query.Where(t =>
                t.RUT.Contains(filter.Termino) ||
                t.Nombre.Contains(filter.Termino) ||
                t.Email.Contains(filter.Termino));
        if (!string.IsNullOrWhiteSpace(filter.Rut))
            query = query.Where(t => t.RUT == filter.Rut);
        if (!string.IsNullOrWhiteSpace(filter.Estado))
            query = query.Where(t => t.Estado == filter.Estado);
        if (filter.FechaInicio.HasValue)
            query = query.Where(t => t.FechaIngreso >= filter.FechaInicio.Value);
        if (filter.FechaFin.HasValue)
            query = query.Where(t => t.FechaIngreso <= filter.FechaFin.Value);
        return query;
    }

    public virtual async Task<T> CrearAsync(T tramite)
    {
        tramite.FechaIngreso = DateTime.UtcNow;
        tramite.FechaActualizacion = DateTime.UtcNow;
        Context.Set<T>().Add(tramite);
        await Context.SaveChangesAsync();
        Logger.LogInformation("Tramite {Tipo} creado con Id {Id}", tramite.TipoModulo, tramite.Id);
        return tramite;
    }

    public virtual async Task<T> ActualizarAsync(T tramite)
    {
        tramite.FechaActualizacion = DateTime.UtcNow;
        Context.Set<T>().Update(tramite);
        await Context.SaveChangesAsync();
        return tramite;
    }

    public virtual async Task<bool> EliminarAsync(int id)
    {
        var tramite = await ObtenerPorIdAsync(id);
        if (tramite is null) return false;
        tramite.Activo = false;
        tramite.FechaEliminacion = DateTime.UtcNow;
        tramite.FechaActualizacion = DateTime.UtcNow;
        await Context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<T> CambiarEstadoAsync(int id, string estado, string usuarioId, string? ip = null)
    {
        var tramite = await ObtenerPorIdAsync(id)
            ?? throw new InvalidOperationException($"Tramite {id} no encontrado");
        tramite.CambiarEstado(estado, usuarioId, ip);
        await Context.SaveChangesAsync();
        return tramite;
    }

    public virtual (bool IsValid, string Message) ValidarTramite(T tramite) => tramite.Validar();

    public virtual async Task<int> ObtenerTotalAsync() =>
        await Context.Set<T>().CountAsync(t => t.Activo);

    public virtual async Task<Dictionary<string, int>> ObtenerEstadisticasPorEstadoAsync() =>
        await Context.Set<T>()
            .Where(t => t.Activo)
            .GroupBy(t => t.Estado)
            .Select(g => new { Estado = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Estado, x => x.Total);
}
