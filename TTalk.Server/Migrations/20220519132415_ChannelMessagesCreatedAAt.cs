using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTalk.Server.Migrations
{
    public partial class ChannelMessagesCreatedAAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ChannelMessages",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ChannelMessages");
        }
    }
}
