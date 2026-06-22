# Gestión de Licencias de Conducir — Municipalidad de Valparaíso

Sistema web para el seguimiento del ciclo de vida completo de trámites de licencias de conducir: citación, carpeta, idoneidad moral, cambio de domicilio, F8/CONASET, otorgamiento, impresión, PAM y entrega.

## Stack tecnológico

| Capa | Tecnología |
|------|------------|
| Framework | .NET 10 |
| Frontend | Blazor Server (Razor Components interactivos) |
| Base de datos | SQLite + Entity Framework Core 10 |
| Reportes PDF | QuestPDF (licencia Community) |
| Estilos | CSS propio (tema institucional) + Bootstrap |

## Estructura del proyecto

```
GestionLicencias.slnx
src/
├── GestionLicencias.Core/            # Dominio: entidades, roles, catálogos, interfaces
│   └── Domain/
│       ├── Entities/                 # TramiteLicencia, Usuario, etc.
│       ├── Interfaces/
│       ├── Catalogos.cs
│       └── Roles.cs
├── GestionLicencias.Infrastructure/  # Persistencia y servicios
│   ├── Migrations/                   # Migraciones EF Core
│   ├── Persistence/                  # GestionLicenciasDbContext
│   ├── Services/                     # UsuarioService, etc.
│   └── Extensions/                   # Registro de dependencias (DI)
└── GestionLicencias.Web/             # Aplicación Blazor Server
    ├── Components/
    │   ├── Layout/
    │   └── Pages/
    │       ├── Admin/                # Gestión de usuarios
    │       └── Modulos/Licencias/    # Listado y formulario de trámites
    ├── Services/                     # SesionUsuario (estado de sesión)
    └── wwwroot/
```

## Requisitos

- **SDK de .NET 10** — [descargar](https://dotnet.microsoft.com/download/dotnet/10.0)
- (Opcional, solo para crear migraciones) herramienta EF Core:

```bash
dotnet tool install --global dotnet-ef
```

No se requiere instalar nada más: los paquetes NuGet se restauran automáticamente al compilar y la base de datos SQLite se crea sola al primer arranque.

## Dependencias NuGet

| Paquete | Versión | Proyecto |
|---------|---------|----------|
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.9 | Infrastructure |
| Microsoft.Extensions.Configuration.Abstractions | 10.0.9 | Infrastructure |
| Microsoft.Extensions.Logging.Abstractions | 10.0.9 | Infrastructure |
| QuestPDF | 2026.6.0 | Infrastructure |
| Microsoft.EntityFrameworkCore.Design | 10.0.9 | Web |

## Cómo ejecutar

```bash
# 1. Restaurar y compilar
dotnet build GestionLicencias.slnx

# 2. Ejecutar la aplicación web
dotnet run --project src/GestionLicencias.Web
```

La aplicación queda disponible en la URL que indique la consola (por defecto `https://localhost:7xxx` / `http://localhost:5xxx`).

Al primer arranque la aplicación:
1. Aplica todas las migraciones pendientes (crea `gestionlicencias.db` en `src/GestionLicencias.Web/`)
2. Crea los usuarios iniciales
3. Carga dos trámites de ejemplo

## Usuarios iniciales

| Usuario | Clave | Rol |
|---------|-------|-----|
| `admin` | `admin123` | Administrador (acceso total, crea el resto de usuarios en `/admin/usuarios`) |
| `director` | `director123` | Director de Tránsito |
| `jefatura` | `jefatura123` | Jefatura de Licencias |

> **Importante**: cambiar estas claves antes de usar el sistema en producción.

## Base de datos

- SQLite, archivo `gestionlicencias.db` (configurable en `src/GestionLicencias.Web/appsettings.json`, clave `ConnectionStrings:DefaultConnection`).
- Las migraciones se aplican automáticamente al arrancar (`db.Database.Migrate()`).
- Para crear una nueva migración tras modificar entidades:

```bash
dotnet ef migrations add NombreDeLaMigracion --project src/GestionLicencias.Infrastructure --startup-project src/GestionLicencias.Web
```

## Desarrollo en contenedor

El repositorio incluye `.devcontainer/` con la configuración para GitHub Codespaces / Dev Containers con .NET 10, y el script `iniciar.sh` para el arranque dentro del contenedor.

## Modo cloud-first (sin operación local)

Si querés operar el proyecto dejando el trabajo en nube (GitHub), usá GitHub Codespaces + GitHub Actions:

1. Abrí el repo en Codespaces (preferiblemente en navegador)
2. Ejecutá, compilá y desarrollá dentro del Codespace
3. Hacé commit/push y validá la CI en GitHub Actions

Guía completa en `GUIA_CLOUD_GITHUB.md`.
