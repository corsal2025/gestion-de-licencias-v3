using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionLicencias.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCambioDomicilioPedidoYCiudad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CambioDomicilioPedido",
                table: "Tramites",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CiudadCambioDomicilio",
                table: "Tramites",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CambioDomicilioPedido",
                table: "Tramites");

            migrationBuilder.DropColumn(
                name: "CiudadCambioDomicilio",
                table: "Tramites");
        }
    }
}
