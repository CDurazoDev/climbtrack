using Microsoft.EntityFrameworkCore.Migrations;

namespace ClimbTrack.Infrastructure.Persistence.Migrations;

public partial class AddCustomSessionBlockItemsJson : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "items_json",
            table: "user_custom_session_blocks",
            type: "text",
            nullable: false,
            defaultValue: "[]");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "items_json",
            table: "user_custom_session_blocks");
    }
}
