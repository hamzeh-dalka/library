using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace library.Migrations
{
    /// <inheritdoc />
    public partial class L2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BorrowDare",
                table: "Borrows",
                newName: "BorrowDate");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Borrows",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Borrows");

            migrationBuilder.RenameColumn(
                name: "BorrowDate",
                table: "Borrows",
                newName: "BorrowDare");
        }
    }
}
