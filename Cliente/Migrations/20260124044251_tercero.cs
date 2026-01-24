using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cliente.Migrations
{
    /// <inheritdoc />
    public partial class tercero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Telefono",
                table: "Clientes",
                newName: "EstadoCivil");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EstadoCivil",
                table: "Clientes",
                newName: "Telefono");
        }
    }
}
