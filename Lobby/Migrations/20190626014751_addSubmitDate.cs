using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lobby.Migrations
{
    public partial class addSubmitDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "submit_date",
                table: "Bill",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "submit_date",
                table: "Bill");
        }
    }
}
