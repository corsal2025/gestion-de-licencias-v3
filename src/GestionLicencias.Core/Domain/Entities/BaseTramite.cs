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

    public byte[]? RowVersion { get; set; }

    public virtual (bool IsValid, string Message) Validar()
    {
        if (string.IsNullOrWhiteSpace(RUT))
            return (false, "El RUT es obligatorio");
        if (string.IsNullOrWhiteSpace(Nombre))
            return (false, "El nombre es obligatorio");

        var (rutValido, rutMsg) = ValidarRut(RUT);
        if (!rutValido)
            return (false, rutMsg);

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

    public static (bool IsValid, string Message) ValidarRut(string rut)
    {
        if (string.IsNullOrWhiteSpace(rut))
            return (false, "El RUT es obligatorio");

        var limpio = rut.Replace(".", "").Replace(",", "").Replace("-", "").Trim().ToUpperInvariant();
        limpio = System.Text.RegularExpressions.Regex.Replace(limpio, @"[^0-9K]", "");

        if (limpio.Length < 2)
            return (false, "RUT inválido: demasiado corto");

        var dvIngresado = limpio.Substring(limpio.Length - 1);
        var cuerpoStr = limpio.Substring(0, limpio.Length - 1);

        if (!long.TryParse(cuerpoStr, out long cuerpo))
            return (false, "RUT inválido: cuerpo no numérico");

        var dvCalculado = CalcularDv(cuerpo);
        var dvEsperado = dvCalculado == 10 ? "K" : dvCalculado.ToString();

        if (!string.Equals(dvIngresado, dvEsperado, StringComparison.OrdinalIgnoreCase))
            return (false, $"RUT inválido: dígito verificador incorrecto (esperado {dvEsperado})");

        return (true, string.Empty);
    }

    private static int CalcularDv(long rut)
    {
        int suma = 0;
        int multiplicador = 2;

        while (rut > 0)
        {
            int digito = (int)(rut % 10);
            suma += digito * multiplicador;
            rut /= 10;
            multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
        }

        int resto = suma % 11;
        int dv = 11 - resto;

        if (dv == 11) return 0;
        if (dv == 10) return 10; // K
        return dv;
    }
}
