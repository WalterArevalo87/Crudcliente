using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cliente.Migrations
{
    /// <inheritdoc />
    public partial class primero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "atms",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreAgencia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClienteModelid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atms", x => x.id);
                    table.ForeignKey(
                        name: "FK_atms_Clientes_ClienteModelid",
                        column: x => x.ClienteModelid,
                        principalTable: "Clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_atms_ClienteModelid",
                table: "atms",
                column: "ClienteModelid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "atms");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
