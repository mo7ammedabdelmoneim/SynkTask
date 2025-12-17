using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynkTask.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentityApplicationUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityApplicationUsers",
                table: "IdentityApplicationUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityApplicationUsers",
                table: "IdentityApplicationUsers",
                column: "IdentityUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityApplicationUsers",
                table: "IdentityApplicationUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityApplicationUsers",
                table: "IdentityApplicationUsers",
                columns: new[] { "IdentityUserId", "ApplicationUserId" });
        }
    }
}
