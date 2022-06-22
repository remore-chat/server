using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTalk.Server.Migrations
{
    public partial class FilesFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_ChannelMessages_ChannelMessageId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_ChannelMessageId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "ChannelMessageId",
                table: "Files");

            migrationBuilder.CreateTable(
                name: "MessageAttachment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MessageId = table.Column<string>(type: "TEXT", nullable: false),
                    FileId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageAttachment_ChannelMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ChannelMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageAttachment_MessageId",
                table: "MessageAttachment",
                column: "MessageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageAttachment");

            migrationBuilder.AddColumn<string>(
                name: "ChannelMessageId",
                table: "Files",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_ChannelMessageId",
                table: "Files",
                column: "ChannelMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_ChannelMessages_ChannelMessageId",
                table: "Files",
                column: "ChannelMessageId",
                principalTable: "ChannelMessages",
                principalColumn: "Id");
        }
    }
}
