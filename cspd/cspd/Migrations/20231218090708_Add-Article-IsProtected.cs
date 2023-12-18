using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Company_Software_Project_Documentation.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleIsProtected : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProtected",
                table: "Articles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProtected",
                table: "Articles");
        }
    }
}
