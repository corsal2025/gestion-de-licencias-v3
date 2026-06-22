namespace GestionLicencias.Core.Domain;

/// <summary>
/// Aggregated data for the statistics page charts.
/// </summary>
public class EstadisticasModelo
{
    public int Total { get; set; }
    public Dictionary<string, int> PorSexo { get; set; } = new();
    public Dictionary<string, int> PorTipoLicencia { get; set; } = new();
    public Dictionary<string, int> PorEstadoCarpeta { get; set; } = new();
    public Dictionary<string, int> PorOtorgamiento { get; set; } = new();
    public Dictionary<string, int> PorLugarAtencion { get; set; } = new();
    public Dictionary<string, int> PorAsistencia { get; set; } = new();
    public List<int> Edades { get; set; } = new();
    public List<PuntoDispersion> Dispersion { get; set; } = new();
}

/// <summary>
/// One scatter-plot point: age vs. processing days (citación → entrega).
/// </summary>
public class PuntoDispersion
{
    public int Edad { get; set; }
    public int DiasTramitacion { get; set; }
}
