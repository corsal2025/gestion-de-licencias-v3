using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionLicencias.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDatosPersonales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Tramites",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sexo",
                table: "Tramites",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoLicencia",
                table: "Tramites",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "Tramites");

            migrationBuilder.DropColumn(
                name: "Sexo",
                table: "Tramites");

            migrationBuilder.DropColumn(
                name: "TipoLicencia",
                table: "Tramites");
        }
    }
}
