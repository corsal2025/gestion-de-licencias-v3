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

    public static string HashClave(string nombreUsuario, string clave) =>
        Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes($"{nombreUsuario.Trim().ToLowerInvariant()}:{clave}")));

    public async Task<Usuario?> ValidarLoginAsync(string nombreUsuario, string clave)
    {
        var usuario = nombreUsuario.Trim().ToLowerInvariant();
        var hash = HashClave(usuario, clave);
        return await _context.Usuarios.FirstOrDefaultAsync(u =>
            u.NombreUsuario == usuario && u.ClaveHash == hash && u.Activo);
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
            ClaveHash = HashClave(usuario, clave),
            NombreCompleto = nombreCompleto.Trim(),
            Rol = rol
        });
        await _context.SaveChangesAsync();
        return (true, $"Usuario '{usuario}' creado.");
    }

    public async Task ActualizarAsync(Usuario usuario, string? nuevaClave = null)
    {
        if (!string.IsNullOrEmpty(nuevaClave))
            usuario.ClaveHash = HashClave(usuario.NombreUsuario, nuevaClave);
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
