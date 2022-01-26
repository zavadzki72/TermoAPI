using Microsoft.EntityFrameworkCore.Migrations;

namespace Termo.API.Migrations
{
    public partial class JsonTryInTryEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JsonTry",
                table: "Tries",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JsonTry",
                table: "Tries");
        }
    }
}
