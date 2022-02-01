using Microsoft.EntityFrameworkCore.Migrations;

namespace Termo.Infrastructure.Migrations
{
    public partial class TriedWorld : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TriedWorld",
                table: "Tries",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TriedWorld",
                table: "Tries");
        }
    }
}
