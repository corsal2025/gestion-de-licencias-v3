namespace GestionLicencias.Core.Domain;

/// <summary>
/// Roles del sistema. Los roles privilegiados (ADMINISTRADOR, DIRECTOR y
/// JEFATURA) tienen acceso total y pueden desbloquear procesos detenidos.
/// El resto de los roles corresponde a una sección de la planilla: el usuario
/// solo puede editar los datos del contribuyente y su sección.
/// </summary>
public static class Roles
{
    public const string Administrador = "ADMINISTRADOR";
    public const string Director = "DIRECTOR";
    public const string Jefatura = "JEFATURA";

    // Roles por sección
    public const string Carpeta = "CARPETA";
    public const string IdoneidadMoral = "IDONEIDAD MORAL";
    public const string CambioDomicilio = "CAMBIO DE DOMICILIO";
    public const string F8 = "F8";
    public const string Otorgamiento = "OTORGAMIENTO E IMPRESIÓN";
    public const string PamEntrega = "PAM Y ENTREGA";

    public static readonly string[] Privilegiados = [Administrador, Director, Jefatura];

    public static readonly string[] Todos =
    [
        Administrador, Director, Jefatura,
        Carpeta, IdoneidadMoral, CambioDomicilio, F8, Otorgamiento, PamEntrega
    ];

    public static bool EsPrivilegiado(string? rol) =>
        rol is Administrador or Director or Jefatura;
}
