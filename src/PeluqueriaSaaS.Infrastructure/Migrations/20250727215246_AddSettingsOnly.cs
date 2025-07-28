using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeluqueriaSaaS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientesBasicos");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombrePeluqueria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DireccionPeluqueria = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TelefonoPeluqueria = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmailPeluqueria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResumenServicioHabilitado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ResumenEncabezado = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ResumenPiePagina = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MostrarLogoEnResumen = table.Column<bool>(type: "bit", nullable: false),
                    RutaLogo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MostrarDatosCliente = table.Column<bool>(type: "bit", nullable: false),
                    MostrarEmpleadoServicio = table.Column<bool>(type: "bit", nullable: false),
                    MostrarFechaHora = table.Column<bool>(type: "bit", nullable: false),
                    MostrarDetalleServicios = table.Column<bool>(type: "bit", nullable: false),
                    MostrarTotalServicio = table.Column<bool>(type: "bit", nullable: false),
                    ColorPrimario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ColorSecundario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TamanoFuente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SimboloMoneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FormatoMoneda = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NotificacionesEmail = table.Column<bool>(type: "bit", nullable: false),
                    BackupAutomatico = table.Column<bool>(type: "bit", nullable: false),
                    DiasRetencionVentas = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CodigoPeluqueria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TemplateCustomHTML = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UsarTemplateCustom = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Settings_CodigoPeluqueria",
                table: "Settings",
                column: "CodigoPeluqueria",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Clientes");

            migrationBuilder.CreateTable(
                name: "ClientesBasicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ciudad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoPostal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsActivo = table.Column<bool>(type: "bit", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UltimaVisita = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesBasicos", x => x.Id);
                });
        }
    }
}
