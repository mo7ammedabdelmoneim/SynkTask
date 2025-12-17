using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynkTask.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityApplicationUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IdentityApplicationUsers",
                columns: table => new
                {
                    IdentityUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityApplicationUsers", x => new { x.IdentityUserId, x.ApplicationUserId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentityApplicationUsers");
        }
    }
}
