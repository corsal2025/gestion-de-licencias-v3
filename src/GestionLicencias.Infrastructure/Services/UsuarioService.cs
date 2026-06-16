using System.Security.Cryptography;
using System.Text;
using GestionLicencias.Core.Domain.Entities;
using GestionLicencias.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionLicencias.Infrastructure.Services;

/// <summary>
/// Administración de usuarios del sistema y validación de credenciales.
/// </summary>
public class UsuarioService
{
    private readonly GestionLicenciasDbContext _context;

    public UsuarioService(GestionLicenciasDbContext context) => _context = context;

    // --- Password hashing (PBKDF2-HMAC-SHA256, per-user random salt) ---
    // Stored format is self-describing: "PBKDF2.{iteraciones}.{saltB64}.{hashB64}".
    private const string PrefijoPbkdf2 = "PBKDF2";
    private const int Iteraciones = 100_000;
    private const int TamanoSalt = 16;
    private const int TamanoHash = 32;

    /// <summary>Genera el hash PBKDF2 de una clave, con salt aleatorio.</summary>
    public static string HashClave(string clave)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            clave, salt, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);
        return $"{PrefijoPbkdf2}.{Iteraciones}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifica una clave contra el hash almacenado. Acepta el formato nuevo
    /// (PBKDF2) y el legado (SHA-256 con el usuario como salt) para no dejar
    /// afuera a las cuentas existentes.
    /// </summary>
    public static bool VerificarClave(string nombreUsuario, string clave, string hashAlmacenado)
    {
        if (string.IsNullOrEmpty(hashAlmacenado)) return false;

        if (hashAlmacenado.StartsWith(PrefijoPbkdf2 + ".", StringComparison.Ordinal))
        {
            var partes = hashAlmacenado.Split('.');
            if (partes.Length != 4 || !int.TryParse(partes[1], out var iteraciones))
                return false;

            byte[] salt, esperado;
            try
            {
                salt = Convert.FromBase64String(partes[2]);
                esperado = Convert.FromBase64String(partes[3]);
            }
            catch (FormatException) { return false; }

            var actual = Rfc2898DeriveBytes.Pbkdf2(
                clave, salt, iteraciones, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(actual, esperado);
        }

        // Legacy SHA-256 (salted with the username).
        var legado = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes($"{nombreUsuario.Trim().ToLowerInvariant()}:{clave}")));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(legado), Encoding.UTF8.GetBytes(hashAlmacenado));
    }

    private static bool EsHashLegado(string hashAlmacenado) =>
        !hashAlmacenado.StartsWith(PrefijoPbkdf2 + ".", StringComparison.Ordinal);

    public async Task<Usuario?> ValidarLoginAsync(string nombreUsuario, string clave)
    {
        var usuario = nombreUsuario.Trim().ToLowerInvariant();
        var encontrado = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == usuario && u.Activo);
        if (encontrado is null) return null;
        if (!VerificarClave(usuario, clave, encontrado.ClaveHash)) return null;

        // Transparent upgrade: re-hash legacy SHA-256 credentials with PBKDF2 on login.
        if (EsHashLegado(encontrado.ClaveHash))
        {
            encontrado.ClaveHash = HashClave(clave);
            await _context.SaveChangesAsync();
        }
        return encontrado;
    }

    public async Task<List<Usuario>> ObtenerTodosAsync() =>
        await _context.Usuarios.OrderBy(u => u.NombreUsuario).ToListAsync();

    public async Task<(bool Ok, string Mensaje)> CrearAsync(
        string nombreUsuario, string clave, string nombreCompleto, string rol)
    {
        var usuario = nombreUsuario.Trim().ToLowerInvariant();
        if (usuario.Length == 0 || clave.Length == 0)
            return (false, "El nombre de usuario y la clave son obligatorios.");
        if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == usuario))
            return (false, $"El usuario '{usuario}' ya existe.");

        _context.Usuarios.Add(new Usuario
        {
            NombreUsuario = usuario,
            ClaveHash = HashClave(clave),
            NombreCompleto = nombreCompleto.Trim(),
            Rol = rol
        });
        await _context.SaveChangesAsync();
        return (true, $"Usuario '{usuario}' creado.");
    }

    public async Task ActualizarAsync(Usuario usuario, string? nuevaClave = null)
    {
        if (!string.IsNullOrEmpty(nuevaClave))
            usuario.ClaveHash = HashClave(nuevaClave);
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null) return false;
        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
        return true;
    }
}
