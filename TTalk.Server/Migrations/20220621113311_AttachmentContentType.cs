using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTalk.Server.Migrations
{
    public partial class AttachmentContentType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "MessageAttachment",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "MessageAttachment");
        }
    }
}
