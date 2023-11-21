﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace trnservice.Migrations
{
    public partial class RemoveRoleFromUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AspNetUsers",
                type: "nvarchar(30)",
                nullable: true);
        }
    }
}
