namespace GestionLicencias.Core.Domain.Entities;

/// <summary>
/// Replica del modelo "MODELO POWERBI": una fila por trámite de licencia de conducir.
/// Las columnas de la planilla se mapean 1:1 a propiedades.
/// </summary>
public class TramiteLicencia : BaseTramite
{
    // Columna 2: APELLIDO (columna 1 NOMBRE y 4 RUT viven en BaseTramite)
    public string? Apellido { get; set; }

    // Columna 3: NOMBRE COMPLETO — en la planilla es una fórmula (mayúsculas de nombre + apellido)
    public string NombreCompleto => $"{Nombre} {Apellido}".Trim().ToUpperInvariant();

    // Columna 5: FECHA DE LA CITACIÓN
    public DateTime? FechaCitacion { get; set; }

    // Columnas 6 y 7 (TEFEFONO / CORREO ELECRTRONICO) viven en BaseTramite (Telefono / Email)

    // Columna 8: LUGAR DE ATENCIÓN
    public string? LugarAtencion { get; set; }

    // Columna 9: ASISTE (SI/NO)
    public string? Asiste { get; set; }

    // Columna 10: FECHA ULTIMA CARPETA
    public DateTime? FechaUltimaCarpeta { get; set; }

    // Columna 11: ESTADO DE LA CARPETA
    public string? EstadoCarpeta { get; set; }

    // Columna 12: FECHA DIGITALIZACION CARPETA
    public DateTime? FechaDigitalizacionCarpeta { get; set; }

    // Columna 13: IDEONIDAD MORAL (APROBADA/REPROBADA)
    public string? IdoneidadMoral { get; set; }

    // Columna 14: CONTACTADO (SI/NO)
    public string? Contactado { get; set; }

    // Columna 15: NOTIFICADO (SI/NO)
    public string? Notificado { get; set; }

    // Columna 16: LUGAR DE ORIGEN CAMBIO DOMICILIO
    public string? LugarOrigenCambioDomicilio { get; set; }

    // Columna 17: PETICION DE CAMBIO DOMICILIO (SI/NO)
    public string? PeticionCambioDomicilio { get; set; }

    // Columna 18: CAMBIO DOMICILIO PEDIDO POR
    public string? CambioDomicilioPedidoPor { get; set; }

    // Columna 19: FOLIO (F8)
    public string? FolioF8 { get; set; }

    // Columna 20: FECHA PENULTIMA CARPETA
    public DateTime? FechaPenultimaCarpeta { get; set; }

    // Columna 21: ESTADO DEL F8
    public string? EstadoF8 { get; set; }

    // Columna 22: OTORGAMIENTO
    public string? Otorgamiento { get; set; }

    // Columna 23: FOLIO LICENCIA
    public string? FolioLicencia { get; set; }

    // Columna 24: FECHA IMPRESION
    public DateTime? FechaImpresion { get; set; }

    // Columna 25: IMPRESA POR
    public string? ImpresaPor { get; set; }

    // Columna 26: BAJADA A PAM (SI/NO)
    public string? BajadaAPam { get; set; }

    // Columna 27: BAJADA POR
    public string? BajadaPor { get; set; }

    // Columna 28: FECHA RECEPCION PAM
    public DateTime? FechaRecepcionPam { get; set; }

    // Columna 29: RECIBIDA EN PAM POR
    public string? RecibidaEnPamPor { get; set; }

    // Columna 30: CONTACTADO CONTRIBUYENTE PARA ENTREGA (SI/NO)
    public string? ContactadoParaEntrega { get; set; }

    // Columna 31: FECHA DE ENTREGA
    public DateTime? FechaEntrega { get; set; }

    // --- Extended columns (appended after the original 31 so clipboard
    //     paste from the source spreadsheet keeps working) ---

    // Columna 32: SEXO
    public string? Sexo { get; set; }

    // Columna 33: FECHA DE NACIMIENTO
    public DateTime? FechaNacimiento { get; set; }

    // Columna 34: TIPO DE LICENCIA (Chilean license classes)
    public string? TipoLicencia { get; set; }

    // Columna 35: CAMBIO DE DOMICILIO PEDIDO (SI/NO) — solo aplica cuando
    // ESTADO DE LA CARPETA es "CAMBIO DE DOMICILIO"
    public string? CambioDomicilioPedido { get; set; }

    // Columna 36: CIUDADES — ciudad de destino del cambio de domicilio
    public string? CiudadCambioDomicilio { get; set; }

    // --- Bloqueo del proceso ---

    // Desbloqueo otorgado por ADMINISTRADOR/DIRECTOR/JEFATURA: levanta el
    // bloqueo aunque persista la condición que lo originó.
    public bool DesbloqueoAdministrativo { get; set; }
    public string? DesbloqueadoPor { get; set; }
    public DateTime? FechaDesbloqueo { get; set; }

    // La idoneidad moral JUZGADO o REPROBADA detiene el proceso.
    public bool IdoneidadBloqueante => IdoneidadMoral is "JUZGADO" or "REPROBADA";

    // El proceso queda bloqueado cuando el contribuyente no asiste a la
    // citación (debe iniciar un trámite nuevo desde cero) o cuando la
    // idoneidad moral resulta JUZGADO/REPROBADA. Solo un desbloqueo
    // administrativo lo levanta.
    public bool ProcesoBloqueado =>
        !DesbloqueoAdministrativo && (Asiste == "NO" || IdoneidadBloqueante);

    public string? MotivoBloqueo =>
        !ProcesoBloqueado ? null
        : Asiste == "NO" ? "NO ASISTIÓ A LA CITACIÓN"
        : $"IDONEIDAD MORAL {IdoneidadMoral}";

    // Las columnas de cambio de domicilio en Carpeta solo se habilitan
    // con este estado exacto.
    public bool CambioDomicilioActivo => EstadoCarpeta == "CAMBIO DE DOMICILIO";

    // Derived from FechaNacimiento; not stored
    public int? Edad => CalcularEdad(FechaNacimiento);

    public static int? CalcularEdad(DateTime? nacimiento)
    {
        if (nacimiento is null) return null;
        var hoy = DateTime.Today;
        var edad = hoy.Year - nacimiento.Value.Year;
        if (nacimiento.Value.Date > hoy.AddYears(-edad)) edad--;
        return edad;
    }
}
