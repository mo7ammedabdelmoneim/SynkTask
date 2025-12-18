using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SynkTask.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectTaskTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_TeamMembers_AssignedMemberId",
                table: "ProjectTasks");

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "ProjectTasks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedMemberId",
                table: "ProjectTasks",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_TeamMembers_AssignedMemberId",
                table: "ProjectTasks",
                column: "AssignedMemberId",
                principalTable: "TeamMembers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_TeamMembers_AssignedMemberId",
                table: "ProjectTasks");

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "ProjectTasks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedMemberId",
                table: "ProjectTasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_TeamMembers_AssignedMemberId",
                table: "ProjectTasks",
                column: "AssignedMemberId",
                principalTable: "TeamMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
