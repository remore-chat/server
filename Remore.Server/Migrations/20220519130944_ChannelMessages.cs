using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remore.Server.Migrations
{
    public partial class ChannelMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChannelMessage_Channels_ChannelId",
                table: "ChannelMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChannelMessage",
                table: "ChannelMessage");

            migrationBuilder.RenameTable(
                name: "ChannelMessage",
                newName: "ChannelMessages");

            migrationBuilder.RenameIndex(
                name: "IX_ChannelMessage_ChannelId",
                table: "ChannelMessages",
                newName: "IX_ChannelMessages_ChannelId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChannelMessages",
                table: "ChannelMessages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelMessages_Channels_ChannelId",
                table: "ChannelMessages",
                column: "ChannelId",
                principalTable: "Channels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChannelMessages_Channels_ChannelId",
                table: "ChannelMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChannelMessages",
                table: "ChannelMessages");

            migrationBuilder.RenameTable(
                name: "ChannelMessages",
                newName: "ChannelMessage");

            migrationBuilder.RenameIndex(
                name: "IX_ChannelMessages_ChannelId",
                table: "ChannelMessage",
                newName: "IX_ChannelMessage_ChannelId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChannelMessage",
                table: "ChannelMessage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelMessage_Channels_ChannelId",
                table: "ChannelMessage",
                column: "ChannelId",
                principalTable: "Channels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
