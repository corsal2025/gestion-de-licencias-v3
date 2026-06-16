using GestionLicencias.Core.Domain;
using GestionLicencias.Core.Domain.Entities;
using GestionLicencias.Core.Domain.Interfaces;
using GestionLicencias.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionLicencias.Infrastructure.Services;

public class TramiteLicenciaService : BaseTramiteService<TramiteLicencia>
{
    public TramiteLicenciaService(GestionLicenciasDbContext context, ILogger<TramiteLicenciaService> logger)
        : base(context, logger) { }

    protected override IQueryable<TramiteLicencia> AplicarFiltros(
        IQueryable<TramiteLicencia> query, BusquedaFilter filter)
    {
        if (filter.SoloActivos)
            query = query.Where(t => t.Activo);
        if (!string.IsNullOrWhiteSpace(filter.Termino))
            query = query.Where(t =>
                t.RUT.Contains(filter.Termino) ||
                t.Nombre.Contains(filter.Termino) ||
                (t.Apellido != null && t.Apellido.Contains(filter.Termino)) ||
                t.Email.Contains(filter.Termino));
        if (!string.IsNullOrWhiteSpace(filter.Rut))
            query = query.Where(t => t.RUT == filter.Rut);
        if (!string.IsNullOrWhiteSpace(filter.EstadoCarpeta))
            query = query.Where(t => t.EstadoCarpeta == filter.EstadoCarpeta);
        if (!string.IsNullOrWhiteSpace(filter.Otorgamiento))
            query = query.Where(t => t.Otorgamiento == filter.Otorgamiento);
        if (filter.FechaInicio.HasValue)
            query = query.Where(t => t.FechaIngreso >= filter.FechaInicio.Value);
        if (filter.FechaFin.HasValue)
            query = query.Where(t => t.FechaIngreso <= filter.FechaFin.Value);
        return query;
    }

    public async Task<Dictionary<string, int>> ObtenerEstadisticasOtorgamientoAsync() =>
        await Context.TramitesLicencia
            .Where(t => t.Activo)
            .GroupBy(t => t.Otorgamiento ?? "SIN OTORGAMIENTO")
            .Select(g => new { Otorgamiento = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Otorgamiento, x => x.Total);

    public async Task<Dictionary<string, int>> ObtenerEstadisticasConasetAsync()
    {
        var hoy = DateTime.Today;
        var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        var registros = await Context.TramitesLicencia
            .Where(t => t.Activo && t.FechaSubidaConaset.HasValue)
            .Select(t => new { t.FechaSubidaConaset })
            .ToListAsync();

        return new Dictionary<string, int>
        {
            ["Subidas CONASET hoy"] = registros.Count(r => r.FechaSubidaConaset!.Value.Date == hoy),
            ["Subidas CONASET semana"] = registros.Count(r => r.FechaSubidaConaset!.Value >= inicioSemana),
            ["Subidas CONASET mes"] = registros.Count(r => r.FechaSubidaConaset!.Value >= inicioMes)
        };
    }

    public async Task<Dictionary<string, int>> ObtenerEstadisticasCarpetaPedidaAsync()
    {
        var total = await Context.TramitesLicencia
            .CountAsync(t => t.Activo && t.CarpetaPedida == "SI");
        return new Dictionary<string, int>
        {
            ["Carpetas pedidas"] = total
        };
    }

    public async Task<EstadisticasModelo> ObtenerEstadisticasModeloAsync()
    {
        var datos = await Context.TramitesLicencia
            .Where(t => t.Activo)
            .Select(t => new
            {
                t.Sexo,
                t.TipoLicencia,
                t.EstadoCarpeta,
                t.Otorgamiento,
                t.LugarAtencion,
                t.FechaNacimiento,
                t.FechaCitacion,
                t.FechaEntrega
            })
            .ToListAsync();

        var modelo = new EstadisticasModelo { Total = datos.Count };

        static Dictionary<string, int> Agrupar(IEnumerable<string?> valores, string sinDato) =>
            valores.GroupBy(v => string.IsNullOrWhiteSpace(v) ? sinDato : v!)
                   .OrderByDescending(g => g.Count())
                   .ToDictionary(g => g.Key, g => g.Count());

        modelo.PorSexo = Agrupar(datos.Select(d => d.Sexo), "SIN REGISTRO");
        modelo.PorTipoLicencia = Agrupar(datos.Select(d => d.TipoLicencia), "SIN REGISTRO");
        modelo.PorEstadoCarpeta = Agrupar(datos.Select(d => d.EstadoCarpeta), "SIN REGISTRO");
        modelo.PorOtorgamiento = Agrupar(datos.Select(d => d.Otorgamiento), "SIN OTORGAMIENTO");
        modelo.PorLugarAtencion = Agrupar(datos.Select(d => d.LugarAtencion), "SIN REGISTRO");

        modelo.Edades = datos
            .Select(d => TramiteLicencia.CalcularEdad(d.FechaNacimiento))
            .Where(e => e.HasValue)
            .Select(e => e!.Value)
            .ToList();

        modelo.Dispersion = datos
            .Where(d => d.FechaNacimiento.HasValue && d.FechaCitacion.HasValue && d.FechaEntrega.HasValue)
            .Select(d => new PuntoDispersion
            {
                Edad = TramiteLicencia.CalcularEdad(d.FechaNacimiento)!.Value,
                DiasTramitacion = Math.Max(0, (d.FechaEntrega!.Value - d.FechaCitacion!.Value).Days)
            })
            .ToList();

        return modelo;
    }
}
