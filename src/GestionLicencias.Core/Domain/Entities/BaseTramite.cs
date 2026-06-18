namespace GestionLicencias.Core.Domain.Entities;

public abstract class BaseTramite
{
    public int Id { get; set; }
    public required string RUT { get; set; }
    public required string Nombre { get; set; }
    public required string Email { get; set; }
    public required string Telefono { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; }
    public string Estado { get; set; } = "En Proceso";
    public string? Observaciones { get; set; }
    public required string UsuarioId { get; set; }
    public required string TipoModulo { get; set; }
    public string? IpOrigen { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime? FechaEliminacion { get; set; }

    public virtual (bool IsValid, string Message) Validar()
    {
        if (string.IsNullOrWhiteSpace(RUT))
            return (false, "El RUT es obligatorio");
        if (string.IsNullOrWhiteSpace(Nombre))
            return (false, "El nombre es obligatorio");
        return (true, string.Empty);
    }

    public void CambiarEstado(string nuevoEstado, string usuarioId, string? ip = null)
    {
        Estado = nuevoEstado;
        UsuarioId = usuarioId;
        IpOrigen = ip;
        FechaActualizacion = DateTime.UtcNow;
    }

    public static string FormatearRut(string rut)
    {
        if (string.IsNullOrWhiteSpace(rut)) return "";
        var limpio = rut.Replace(".", "").Replace(",", "").Replace("-", "").Trim().ToUpperInvariant();
        limpio = System.Text.RegularExpressions.Regex.Replace(limpio, @"[^0-9K]", "");
        if (limpio.Length < 2) return limpio;
        var dv = limpio.Substring(limpio.Length - 1);
        var cuerpoStr = limpio.Substring(0, limpio.Length - 1);
        if (long.TryParse(cuerpoStr, out long cuerpo))
        {
            var nfi = new System.Globalization.NumberFormatInfo
            {
                NumberGroupSeparator = ".",
                NumberDecimalSeparator = ","
            };
            return cuerpo.ToString("N0", nfi) + "-" + dv;
        }
        return cuerpoStr + "-" + dv;
    }
}
