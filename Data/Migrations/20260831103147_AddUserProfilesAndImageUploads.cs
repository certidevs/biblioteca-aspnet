using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaAspNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfilesAndImageUploads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverImageFileName",
                table: "Books",
                type: "TEXT",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarFileName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 260,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImageFileName",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "AvatarFileName",
                table: "AspNetUsers");
        }
    }
}
