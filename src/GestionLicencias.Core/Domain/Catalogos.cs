namespace GestionLicencias.Core.Domain;

/// <summary>
/// Listas de valores tomadas textualmente de las listas desplegables
/// de la planilla "MODELO POWERBI". No modificar sin actualizar el modelo.
/// </summary>
public static class Catalogos
{
    public static readonly string[] SiNo = ["SI", "NO"];

    public static readonly string[] LugaresAtencion =
    [
        "PLACILLA",
        "AV. ARGENTINA",
        "MERCADO PUERTO"
    ];

    public static readonly string[] EstadosCarpeta =
    [
        "1° LICENCIA",
        "SUBIDA A CONASET",
        "SUBIDA CON F8",
        "CAMBIO DOM. SUBIDO A CONASET",
        "CAMBIO DOM. SUBIDO CON CORREO",
        "SUBIDA CON OFICIO",
        "SE ENCUENTRA EN ARCHIVOS",
        "SE ENCUENTRA EN OF.43",
        "CAMBIO DE DOMICILIO SOLICITADO",
        "CAMBIO DE DOMICILIO",
        "NO EXISTE CARPETA",
        "CREAR OFICIO",
        "CANJE LIC. EXTRANJERA"
    ];

    public static readonly string[] IdoneidadMoral = ["APROBADA", "REPROBADA"];

    public static readonly string[] CambioDomicilioPedidoPor =
    [
        "ALEXANDRA",
        "ANA",
        "LEONOR"
    ];

    public static readonly string[] EstadosF8 =
    [
        "SUBIDA A CONASET",
        "CREAR OFICIO",
        "CAMBIO DE DOMICILIO",
        "PRIMERA LICENCIA"
    ];

    public static readonly string[] Otorgamientos =
    [
        "OTORGADO",
        "DENEGADO",
        "ESPERA EXÁMEN",
        "S/SGL"
    ];

    public static readonly string[] ImpresaPor =
    [
        "JOSELYN",
        "JAVIERA",
        "LISA",
        "SANDRA",
        "KARINA"
    ];

    public static readonly string[] RecibidaEnPamPor = ["SERGIO", "TAMARA"];

    // Extended catalogs (not part of the original spreadsheet)
    public static readonly string[] Sexos = ["MASCULINO", "FEMENINO"];

    public static readonly string[] TiposLicencia =
    [
        "A1", "A2", "A3", "A4", "A5",
        "B", "C", "D", "E", "F"
    ];
}
