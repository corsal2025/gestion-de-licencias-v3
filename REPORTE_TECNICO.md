# Reporte Técnico — Gestión de Licencias v3

**Proyecto**: Municipalidad de Valparaíso — Gestión de Licencias de Conducir
**Framework**: .NET 10.0 / Blazor Server
**Base de datos**: SQLite (`gestionlicencias.db`)
**Último commit**: `a635a6f` (2026-06-16)

---

## 1. Arquitectura

```
┌─────────────────────────────────────────┐
│  GestionLicencias.Web                   │  Blazor Server (Razor Components)
│  net10.0 — Microsoft.NET.Sdk.Web        │  Puerto: 5029 (http) / 7189 (https)
├─────────────────────────────────────────┤
│  GestionLicencias.Infrastructure        │  EF Core, servicios, migraciones
│  net10.0 — Microsoft.NET.Sdk            │
├─────────────────────────────────────────┤
│  GestionLicencias.Core                  │  Entidades, interfaces, catálogos
│  net10.0 — Microsoft.NET.Sdk            │
└─────────────────────────────────────────┘
```

**Dependency graph**: `Web → Infrastructure → Core`

---

## 2. Paquetes NuGet

| Paquete | Versión | Capa | Propósito |
|---------|---------|------|-----------|
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.9 | Infrastructure | Proveedor SQLite para EF Core |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.9 | Web (design-time) | Migraciones EF Core (`dotnet ef`) |
| `Microsoft.Extensions.Configuration.Abstractions` | 10.0.9 | Infrastructure | Abstracción `IConfiguration` |
| `Microsoft.Extensions.Logging.Abstractions` | 10.0.9 | Infrastructure | Abstracción `ILogger` |
| `QuestPDF` | 2026.6.0 | Infrastructure | Generación de PDFs (Community License) |

---

## 3. Referencias entre Proyectos

| Origen | Destino | Tipo |
|--------|---------|------|
| `GestionLicencias.Web` | `GestionLicencias.Infrastructure` | ProjectReference |
| `GestionLicencias.Infrastructure` | `GestionLicencias.Core` | ProjectReference |

---

## 4. Configuración

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=gestionlicencias.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### launchSettings.json
- **http**: `http://localhost:5029`
- **https**: `https://localhost:7189`
- **Environment**: `Development`

---

## 5. Servicios Registrados (DI)

| Servicio | Vida | Detalle |
|----------|------|---------|
| `GestionLicenciasDbContext` | Scoped | SQLite via `UseSqlite()` |
| `TramiteLicenciaService` | Scoped | Lógica de negocio — licencias |
| `ITramiteService<TramiteLicencia>` | Scoped | Fábrica delegada a `TramiteLicenciaService` |
| `UsuarioService` | Scoped | Gestión de usuarios, hashing, auth |
| `SesionUsuario` | Scoped | Estado de sesión del usuario conectado |

---

## 6. Middleware Pipeline

| Orden | Middleware | Condición |
|-------|-----------|-----------|
| 1 | `UseExceptionHandler("/Error")` | Solo Production |
| 2 | `UseHsts()` | Solo Production |
| 3 | `UseStatusCodePagesWithReExecute("/not-found")` | Siempre |
| 4 | `UseHttpsRedirection()` | Siempre |
| 5 | `UseAntiforgery()` | Siempre |
| 6 | `MapStaticAssets()` | Siempre |
| 7 | `MapRazorComponents<App>().AddInteractiveServerRenderMode()` | Siempre |

---

## 7. Seed Data (Arranque)

Al iniciar por primera vez, después de `db.Database.Migrate()`:

### Usuarios (3)
| Usuario | Rol | Contraseña |
|---------|-----|------------|
| `admin` | Administrador | `admin123` |
| `director` | Director | `director123` |
| `jefatura` | Jefatura | `jefatura123` |

### Trámites de prueba (2)
- RUT `19.336.934-0`
- RUT `20.761.157-3`

---

## 8. Entidades Principales

### TramiteLicencia (38 columnas + metadata)

| # | Campo | Tipo | Descripción |
|---|-------|------|-------------|
| 1 | Nombre | string | Nombre del contribuyente |
| 2 | Apellido | string | Apellido del contribuyente |
| 3 | RUT | string | RUT chileno |
| 4 | FechaCitacion | DateTime? | Fecha de citación |
| 5 | Telefono | string | Teléfono (+56 format) |
| 6 | Email | string | Correo electrónico |
| 7 | LugarAtencion | string | PLACILLA / AV. ARGENTINA / MERCADO PUERTO |
| 8 | Asiste | string | SI/NO — asistencia a citación |
| 9 | FechaUltimaCarpeta | DateTime? | Fecha última carpeta física |
| 10 | EstadoCarpeta | string | Estado de la carpeta |
| 11 | CambioDomicilioPedido | string | SI/NO — cambio de domicilio |
| 12 | CiudadCambioDomicilio | string | Ciudad destino (346 ciudades chilenas) |
| 13 | FechaDigitalizacionCarpeta | DateTime? | Fecha de digitalización |
| 14 | **FechaSubidaConaset** | DateTime? | **NUEVO** — auto-set al "SUBIDA A CONASET" |
| 15 | **CarpetaPedida** | string | **NUEVO** — SI/NO |
| 16 | IdoneidadMoral | string | Idoneidad moral |
| 17 | Contactado | string | SI/NO |
| 18 | Notificado | string | SI/NO |
| 19 | LugarOrigenCambioDomicilio | string | Lugar de origen |
| 20 | PeticionCambioDomicilio | string | SI/NO |
| 21 | CambioDomicilioPedidoPor | string | Quién pidió el cambio |
| 22 | FolioF8 | string | Folio del F8 |
| 23 | FechaPenultimaCarpeta | DateTime? | Fecha penúltima carpeta |
| 24 | EstadoF8 | string | Estado del F8 |
| 25 | Otorgamiento | string | OTORGADO / DENEGADO / ESPERA EXAMEN |
| 26 | FolioLicencia | string | Folio de licencia |
| 27 | FechaImpresion | DateTime? | Fecha de impresión |
| 28 | ImpresaPor | string | Quién imprimió |
| 29 | BajadaAPam | string | SI/NO |
| 30 | BajadaPor | string | Quién bajó a PAM |
| 31 | FechaRecepcionPam | DateTime? | Fecha recepción PAM |
| 32 | RecibidaEnPamPor | string | Quién recibió en PAM |
| 33 | ContactadoParaEntrega | string | SI/NO |
| 34 | FechaEntrega | DateTime? | Fecha de entrega |
| 35 | Sexo | string | Sexo |
| 36 | FechaNacimiento | DateTime? | Fecha nacimiento |
| 37 | TipoLicencia | string | Tipo de licencia |
| 38 | Observaciones | string | Observaciones |

### Campos de bloqueo (BaseTramite)
- `DesbloqueoAdministrativo` (bool) — desbloqueo por admin
- `DesbloqueadoPor` (string) — quién desbloqueó
- `FechaDesbloqueo` (DateTime?) — cuándo desbloqueó
- `ProcesoBloqueado` (computed) — true si Asiste=NO o Idoneidad=JUZGADO/REPROBADA
- `MotivoBloqueo` (computed) — razón del bloqueo

### Otorgamientos del catálogo
- OTORGADO, DENEGADO, ESPERA EXAMEN, S/SGL

### Estados de Carpeta del catálogo
- SE ENCUENTRA EN OF.43
- SE ENCUENTRA EN ARCHIVOS
- SUBIDA A CONASET
- SUBIDA CON F8
- (y otros)

---

## 9. Funcionalidades Implementadas

### Frontend (Blazor Server)

| Módulo | Funcionalidad |
|--------|--------------|
| **LicenciasList** | Tabla paginada (50/página), inline editing, búsqueda con formateo RUT automático, filtro por otorgamiento y estado carpeta |
| **LicenciasForm** | Formulario de creación/edición con auto-asignación de EstadoCarpeta y FechaSubidaConaset |
| **Autocomplete ciudades** | 346 ciudades chilenas con filtro por texto (máx 20 sugerencias) |
| **Toast "NO domicilio"** | Aviso visual al setear CambioDomicilioPedido = NO |
| **PDF modal** | 3 opciones: Última Carpeta, Penúltima Carpeta, Impresas + filtro TipoListado (Oficina43/Archivos/NoSolicitadas/Todos) |
| **FechaNacimientoSelector** | Componente dedicado para fecha de nacimiento |
| **Permisos por rol** | `NoEdita(sección)` — secciones bloqueadas según rol del usuario |
| **Desbloquear** | Botón para ADMINISTRADOR/DIRECTOR/JEFATURA cuando ProcesoBloqueado=true |
| **Exportar Excel** | Genera `.xls` con HTML table |
| **Pegar desde Excel** | Importación masiva desde clipboard (TSV) |
| **Dominios de correo** | Gestión de dominios sugeridos para autocompletado email |
| **Estadísticas** | Otorgamiento + CONASET (hoy/semana/mes) + Carpetas pedidas |
| **Seguimiento** | Módulo dedicado |
| **Login** | Autenticación con sesión |

### Backend (Services)

| Servicio | Métodos clave |
|----------|--------------|
| `TramiteLicenciaService` | `BuscarAsync`, `CrearAsync`, `ActualizarAsync`, `EliminarAsync`, `ObtenerEstadisticasOtorgamientoAsync`, `ObtenerEstadisticasConasetAsync`, `ObtenerEstadisticasCarpetaPedidaAsync` |
| `UsuarioService` | CRUD usuarios, hashing contraseñas |
| `BaseTramiteService<T>` | CRUD genérico para trámites |
| `ReporteCarpetasPdf` | Genera PDF de carpetas (última/penúltima) |
| `ReporteImpresasPdf` | Genera PDF de impresas por lugar/funcionaria |
| `LogoValpo` | Logo de la municipalidad para PDFs |

### Validaciones
- `ValidarTramite()` — retorna `(bool ok, string msg)`
- Normalización de teléfono: siempre `+56 - XXXXXXXXX`
- Formateo RUT automático en búsqueda

---

## 10. Archivos Configurados en .gitignore

```
bin/, obj/, publish/           # Build output
.vs/, .vscode/, .idea/         # IDE
*.user, *.suo                  # VS user files
*.db, *.db-shm, *.db-wal      # SQLite (datos personales/RUTs)
*.log, .atl/                   # Logs
```

---

## 11. Infraestructura

- **Sin Docker** — no hay Dockerfile ni docker-compose
- **Sin devcontainer** — archivo existe pero no configurado
- **SQLite local** — archivo `gestionlicencias.db` en directorio de trabajo
- **Blazor Server** — rendering interactivo server-side (SignalR)
- **EF Core Migrations** — 5 migraciones aplicadas

---

## 12. Stack Tecnológico

| Capa | Tecnología |
|------|------------|
| Runtime | .NET 10.0 |
| Frontend | Blazor Server (Razor Components) |
| ORM | Entity Framework Core 10.0.9 |
| Base de datos | SQLite |
| PDF | QuestPDF 2026.6.0 |
| Autenticación | Custom (SesionUsuario + AuthGuard) |
| Estado | Scoped services |
| Build | MSBuild / dotnet CLI |
