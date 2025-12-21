using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynkTask.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToTeamLeadAndTeamMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "TeamMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "TeamLeads",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "TeamLeads");

            migrationBuilder.AddColumn<Guid>(
                name: "TeamMemberId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TeamMemberId",
                table: "Projects",
                column: "TeamMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_TeamMembers_TeamMemberId",
                table: "Projects",
                column: "TeamMemberId",
                principalTable: "TeamMembers",
                principalColumn: "Id");
        }
    }
}
