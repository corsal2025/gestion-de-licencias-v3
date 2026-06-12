using GestionLicencias.Core.Domain;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace GestionLicencias.Web.Services;

/// <summary>
/// Sesión del usuario conectado (por circuito), persistida en el navegador
/// con almacenamiento protegido para sobrevivir recargas de página.
/// </summary>
public class SesionUsuario
{
    public record Info(int Id, string NombreUsuario, string NombreCompleto, string Rol);

    private const string ClaveStorage = "gl-sesion";
    private readonly ProtectedLocalStorage _storage;

    public SesionUsuario(ProtectedLocalStorage storage) => _storage = storage;

    public Info? Actual { get; private set; }
    public bool Cargada { get; private set; }

    /// <summary>Carga la sesión desde el navegador. Solo es posible después del primer render.</summary>
    public async Task<Info?> CargarAsync()
    {
        if (!Cargada)
        {
            try
            {
                var resultado = await _storage.GetAsync<Info>(ClaveStorage);
                Actual = resultado.Success ? resultado.Value : null;
            }
            catch
            {
                Actual = null; // datos corruptos o clave de protección rotada
            }
            Cargada = true;
        }
        return Actual;
    }

    public async Task IniciarAsync(Info info)
    {
        Actual = info;
        Cargada = true;
        await _storage.SetAsync(ClaveStorage, info);
    }

    public async Task CerrarAsync()
    {
        Actual = null;
        await _storage.DeleteAsync(ClaveStorage);
    }

    /// <summary>ADMINISTRADOR, DIRECTOR y JEFATURA: acceso total.</summary>
    public bool EsPrivilegiado => Roles.EsPrivilegiado(Actual?.Rol);

    /// <summary>Los datos del contribuyente los puede editar cualquier usuario conectado.</summary>
    public bool PuedeEditarContribuyente => Actual is not null;

    /// <summary>Una sección solo la edita su rol correspondiente o un rol privilegiado.</summary>
    public bool PuedeEditar(string seccion) =>
        EsPrivilegiado || Actual?.Rol == seccion;
}
