using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionLicencias.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tramites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RUT = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: true),
                    UsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    TipoModulo = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    IpOrigen = table.Column<string>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Apellido = table.Column<string>(type: "TEXT", nullable: true),
                    FechaCitacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LugarAtencion = table.Column<string>(type: "TEXT", nullable: true),
                    Asiste = table.Column<string>(type: "TEXT", nullable: true),
                    FechaUltimaCarpeta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstadoCarpeta = table.Column<string>(type: "TEXT", nullable: true),
                    FechaDigitalizacionCarpeta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IdoneidadMoral = table.Column<string>(type: "TEXT", nullable: true),
                    Contactado = table.Column<string>(type: "TEXT", nullable: true),
                    Notificado = table.Column<string>(type: "TEXT", nullable: true),
                    LugarOrigenCambioDomicilio = table.Column<string>(type: "TEXT", nullable: true),
                    PeticionCambioDomicilio = table.Column<string>(type: "TEXT", nullable: true),
                    CambioDomicilioPedidoPor = table.Column<string>(type: "TEXT", nullable: true),
                    FolioF8 = table.Column<string>(type: "TEXT", nullable: true),
                    FechaPenultimaCarpeta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstadoF8 = table.Column<string>(type: "TEXT", nullable: true),
                    Otorgamiento = table.Column<string>(type: "TEXT", nullable: true),
                    FolioLicencia = table.Column<string>(type: "TEXT", nullable: true),
                    FechaImpresion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ImpresaPor = table.Column<string>(type: "TEXT", nullable: true),
                    BajadaAPam = table.Column<string>(type: "TEXT", nullable: true),
                    BajadaPor = table.Column<string>(type: "TEXT", nullable: true),
                    FechaRecepcionPam = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RecibidaEnPamPor = table.Column<string>(type: "TEXT", nullable: true),
                    ContactadoParaEntrega = table.Column<string>(type: "TEXT", nullable: true),
                    FechaEntrega = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tramites", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tramites_RUT",
                table: "Tramites",
                column: "RUT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tramites");
        }
    }
}
