using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalPageSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "Hobbies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateTable(
                name: "PersonalPageSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    PageSlug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SectionOrder = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SectionVisibility = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SectionTemplates = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Theme = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalPageSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalPageSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalPageSettings_UserId",
                table: "PersonalPageSettings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalPageSettings");

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "Hobbies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
