using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JwtCleanArch.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class profile_image_added_on_application_usertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileImagePath",
                table: "ApplicationUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImagePath",
                table: "ApplicationUsers");
        }
    }
}
