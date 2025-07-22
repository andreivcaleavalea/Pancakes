START TRANSACTION;
ALTER TABLE "BlogPosts" ALTER COLUMN "AuthorId" TYPE text;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250722084539_UpdateAuthorIdToString', '9.0.0');

COMMIT;

