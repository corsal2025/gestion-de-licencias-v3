using GestionLicencias.Core.Domain;
using GestionLicencias.Core.Domain.Entities;
using GestionLicencias.Infrastructure.Extensions;
using GestionLicencias.Infrastructure.Persistence;
using GestionLicencias.Infrastructure.Services;
using GestionLicencias.Web.Components;
using GestionLicencias.Web.Services;
using Microsoft.EntityFrameworkCore;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddGestionLicenciasInfrastructure(builder.Configuration);
builder.Services.AddScoped<SesionUsuario>();

var app = builder.Build();

// Apply pending migrations and seed the sample rows that come from the source spreadsheet.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GestionLicenciasDbContext>();
    db.Database.Migrate();

    // Limpieza de registros fantasma de cabecera de pruebas anteriores
    var fantasmas = db.TramitesLicencia.Where(t => t.RUT == "RUT" || t.Nombre == "NOMBRE COMPLETO" || t.Nombre == "NOMBRE").ToList();
    if (fantasmas.Any())
    {
        db.TramitesLicencia.RemoveRange(fantasmas);
        db.SaveChanges();
    }

    // Usuarios iniciales con acceso total. El administrador puede crear el
    // resto de los usuarios (uno por sección) desde /admin/usuarios.
    if (!db.Usuarios.Any())
    {
        db.Usuarios.AddRange(
            new Usuario
            {
                NombreUsuario = "admin",
                ClaveHash = UsuarioService.HashClave("admin123"),
                NombreCompleto = "Administrador del Sistema",
                Rol = Roles.Administrador
            },
            new Usuario
            {
                NombreUsuario = "director",
                ClaveHash = UsuarioService.HashClave("director123"),
                NombreCompleto = "Director de Tránsito",
                Rol = Roles.Director
            },
            new Usuario
            {
                NombreUsuario = "jefatura",
                ClaveHash = UsuarioService.HashClave("jefatura123"),
                NombreCompleto = "Jefatura de Licencias",
                Rol = Roles.Jefatura
            });
        db.SaveChanges();
    }

    if (!db.TramitesLicencia.Any())
    {
        db.TramitesLicencia.AddRange(
            new TramiteLicencia
            {
                RUT = "19.336.934-0",
                Nombre = "Gustavo Ariel",
                Apellido = "Escobar Sepúlveda",
                Email = "",
                Telefono = "",
                UsuarioId = "SISTEMA",
                TipoModulo = "TramiteLicencia",
                FechaCitacion = new DateTime(2026, 5, 11),
                LugarAtencion = "PLACILLA",
                Asiste = "SI",
                EstadoCarpeta = "1° LICENCIA",
                IdoneidadMoral = "APROBADA",
                Contactado = "SI",
                Notificado = "SI",
                PeticionCambioDomicilio = "SI",
                CambioDomicilioPedidoPor = "ALEXANDRA",
                EstadoF8 = "SUBIDA A CONASET",
                Otorgamiento = "OTORGADO",
                ImpresaPor = "JOSELYN",
                BajadaAPam = "SI",
                RecibidaEnPamPor = "SERGIO",
                ContactadoParaEntrega = "SI"
            },
            new TramiteLicencia
            {
                RUT = "20.761.157-3",
                Nombre = "Sofía Catalina",
                Apellido = "Mansilla Osorio",
                Email = "",
                Telefono = "",
                UsuarioId = "SISTEMA",
                TipoModulo = "TramiteLicencia",
                FechaCitacion = new DateTime(2026, 5, 6),
                LugarAtencion = "AV. ARGENTINA",
                Asiste = "NO",
                EstadoCarpeta = "SUBIDA A CONASET",
                IdoneidadMoral = "REPROBADA",
                Contactado = "NO",
                Notificado = "NO",
                PeticionCambioDomicilio = "NO",
                CambioDomicilioPedidoPor = "ANA",
                EstadoF8 = "CREAR OFICIO",
                Otorgamiento = "DENEGADO",
                ImpresaPor = "JAVIERA",
                BajadaAPam = "NO",
                RecibidaEnPamPor = "TAMARA",
                ContactadoParaEntrega = "NO"
            });
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
