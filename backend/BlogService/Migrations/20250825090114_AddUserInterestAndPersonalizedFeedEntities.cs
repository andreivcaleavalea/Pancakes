using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogService.Migrations
{
    /// <inheritdoc />
    public partial class AddUserInterestAndPersonalizedFeedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonalizedFeeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    BlogPostIds = table.Column<List<Guid>>(type: "jsonb", nullable: false),
                    Scores = table.Column<List<double>>(type: "jsonb", nullable: false),
                    AlgorithmVersion = table.Column<string>(type: "text", nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalizedFeeds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserInterests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Tag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: false),
                    InteractionCount = table.Column<int>(type: "integer", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInterests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalizedFeeds_ComputedAt",
                table: "PersonalizedFeeds",
                column: "ComputedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalizedFeeds_ExpiresAt",
                table: "PersonalizedFeeds",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalizedFeeds_UserId",
                table: "PersonalizedFeeds",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_LastUpdated",
                table: "UserInterests",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_UserId",
                table: "UserInterests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_UserId_Tag",
                table: "UserInterests",
                columns: new[] { "UserId", "Tag" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalizedFeeds");

            migrationBuilder.DropTable(
                name: "UserInterests");
        }
    }
}
