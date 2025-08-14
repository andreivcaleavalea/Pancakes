using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogService.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReportForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the problematic foreign key constraints
            migrationBuilder.DropForeignKey(
                name: "FK_Report_BlogPost",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_Comment",
                table: "Reports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate the foreign key constraints if rolling back
            migrationBuilder.AddForeignKey(
                name: "FK_Report_BlogPost",
                table: "Reports",
                column: "ContentId",
                principalTable: "BlogPosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Report_Comment",
                table: "Reports",
                column: "ContentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
