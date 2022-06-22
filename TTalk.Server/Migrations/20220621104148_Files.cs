using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTalk.Server.Migrations
{
    public partial class Files : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ChannelMessageId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_ChannelMessages_ChannelMessageId",
                        column: x => x.ChannelMessageId,
                        principalTable: "ChannelMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_ChannelMessageId",
                table: "Files",
                column: "ChannelMessageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files");
        }
    }
}
