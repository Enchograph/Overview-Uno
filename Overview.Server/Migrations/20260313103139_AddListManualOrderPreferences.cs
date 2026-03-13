using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Overview.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddListManualOrderPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ListManualOrder",
                table: "user_settings",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListManualOrder",
                table: "user_settings");
        }
    }
}
