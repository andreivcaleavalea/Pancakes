using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExistingBlogPostsWithEmptyTags : Migration
    {
            /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Update all existing BlogPosts with null Tags to have empty JSON arrays
        migrationBuilder.Sql("UPDATE \"BlogPosts\" SET \"Tags\" = '[]'::jsonb WHERE \"Tags\" IS NULL;");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Set all Tags back to null (if needed for rollback)
        migrationBuilder.Sql("UPDATE \"BlogPosts\" SET \"Tags\" = NULL;");
    }
    }
}
