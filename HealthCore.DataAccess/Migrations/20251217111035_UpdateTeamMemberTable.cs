using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynkTask.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeamMemberTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_TeamLeads_TeamLeadId",
                table: "TeamMembers");

            migrationBuilder.AlterColumn<Guid>(
                name: "TeamLeadId",
                table: "TeamMembers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "TeamMembers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_TeamLeads_TeamLeadId",
                table: "TeamMembers",
                column: "TeamLeadId",
                principalTable: "TeamLeads",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_TeamLeads_TeamLeadId",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "TeamMembers");

            migrationBuilder.AlterColumn<Guid>(
                name: "TeamLeadId",
                table: "TeamMembers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_TeamLeads_TeamLeadId",
                table: "TeamMembers",
                column: "TeamLeadId",
                principalTable: "TeamLeads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
