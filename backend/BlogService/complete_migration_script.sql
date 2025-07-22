CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "BlogPosts" (
    "Id" uuid NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Content" text NOT NULL,
    "FeaturedImage" character varying(500),
    "Status" integer NOT NULL,
    "AuthorId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "PublishedAt" timestamp with time zone,
    CONSTRAINT "PK_BlogPosts" PRIMARY KEY ("Id")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250716120546_InitialCleanModel', '9.0.0');

CREATE TABLE "Comments" (
    "Id" uuid NOT NULL,
    "Content" text NOT NULL,
    "AuthorName" text NOT NULL,
    "BlogPostId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Comments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Comments_BlogPosts_BlogPostId" FOREIGN KEY ("BlogPostId") REFERENCES "BlogPosts" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Comments_BlogPostId" ON "Comments" ("BlogPostId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250721080507_AddCommentsTable', '9.0.0');

ALTER TABLE "Comments" ADD "ParentCommentId" uuid;

CREATE INDEX "IX_Comments_ParentCommentId" ON "Comments" ("ParentCommentId");

ALTER TABLE "Comments" ADD CONSTRAINT "FK_Comments_Comments_ParentCommentId" FOREIGN KEY ("ParentCommentId") REFERENCES "Comments" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250721081040_AddCommentReplies', '9.0.0');

CREATE TABLE "CommentLikes" (
    "Id" uuid NOT NULL,
    "CommentId" uuid NOT NULL,
    "UserIdentifier" character varying(100) NOT NULL,
    "IsLike" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CommentLikes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CommentLikes_Comments_CommentId" FOREIGN KEY ("CommentId") REFERENCES "Comments" ("Id") ON DELETE CASCADE
);

CREATE TABLE "PostRatings" (
    "Id" uuid NOT NULL,
    "BlogPostId" uuid NOT NULL,
    "UserIdentifier" character varying(100) NOT NULL,
    "Rating" numeric NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_PostRatings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PostRatings_BlogPosts_BlogPostId" FOREIGN KEY ("BlogPostId") REFERENCES "BlogPosts" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_CommentLikes_CommentId_UserIdentifier" ON "CommentLikes" ("CommentId", "UserIdentifier");

CREATE UNIQUE INDEX "IX_PostRatings_BlogPostId_UserIdentifier" ON "PostRatings" ("BlogPostId", "UserIdentifier");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250721090834_AddRatingAndLikeSystem', '9.0.0');

ALTER TABLE "BlogPosts" ALTER COLUMN "AuthorId" TYPE text;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250722084539_UpdateAuthorIdToString', '9.0.0');

COMMIT;

