using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISW2_Primer_parcial.Migrations
{
    /// <inheritdoc />
    public partial class AddCodigoProductoYEliminado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoProducto",
                table: "Productos",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Eliminado",
                table: "Productos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                "UPDATE Productos SET CodigoProducto = 'PRD-' || IdProducto WHERE CodigoProducto IS NULL OR CodigoProducto = '';"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CodigoProducto",
                table: "Productos",
                column: "CodigoProducto",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_CodigoProducto",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CodigoProducto",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Eliminado",
                table: "Productos");
        }
    }
}
