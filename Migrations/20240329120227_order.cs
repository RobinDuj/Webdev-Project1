using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projectfiets.Migrations
{
    public partial class order : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustommerID",
                table: "Orders",
                newName: "CustomerID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomerID",
                table: "Orders",
                newName: "CustommerID");
        }
    }
}
