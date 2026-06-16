using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionLicencias.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSubidaConasetYPedida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CarpetaPedida",
                table: "Tramites",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaSubidaConaset",
                table: "Tramites",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarpetaPedida",
                table: "Tramites");

            migrationBuilder.DropColumn(
                name: "FechaSubidaConaset",
                table: "Tramites");
        }
    }
}
