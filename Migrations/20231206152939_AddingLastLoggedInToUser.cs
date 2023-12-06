using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace trnservice.Migrations
{
    public partial class AddingLastLoggedInToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoggedIn",
                table: "AspNetUsers",
                type: "datetime",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLoggedIn",
                table: "AspNetUsers");
        }
    }
}
