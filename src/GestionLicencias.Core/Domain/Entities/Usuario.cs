namespace GestionLicencias.Core.Domain.Entities;

/// <summary>
/// Usuario del sistema con su rol de acceso (ver <see cref="Roles"/>).
/// </summary>
public class Usuario
{
    public int Id { get; set; }
    public required string NombreUsuario { get; set; }
    public required string ClaveHash { get; set; }
    public required string NombreCompleto { get; set; }
    public required string Rol { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
