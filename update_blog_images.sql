-- Script to Update Blog Post Images to use Lorem Picsum format
-- Changes URLs from example.com/prnt.sc to https://picsum.photos/id/XXX/640/480
-- Where XXX is a random 3-digit number (Lorem Picsum ID format)
--
-- Why Lorem Picsum:
-- ✅ Designed for hotlinking/embedding - that's its purpose
-- ✅ Simple URL pattern: picsum.photos/id/[number]/[width]/[height]
-- ✅ Real high-quality photos, not placeholders
-- ✅ Fast, reliable CDN service
-- ✅ Perfect for blog post featured images

-- Function to generate random 3-digit number for Lorem Picsum ID (1-999)
CREATE OR REPLACE FUNCTION generate_random_code() 
RETURNS TEXT AS $$
BEGIN
    RETURN (floor(random() * 999) + 1)::text;
END;
$$ LANGUAGE plpgsql;

-- Update all blog post images to use Lorem Picsum format
UPDATE "BlogPosts" 
SET "FeaturedImage" = 'https://picsum.photos/id/' || generate_random_code() || '/640/480'
WHERE "FeaturedImage" LIKE 'https://example.com/%';

-- Update existing prnt.sc URLs to use Lorem Picsum format
UPDATE "BlogPosts" 
SET "FeaturedImage" = 'https://picsum.photos/id/' || generate_random_code() || '/640/480'
WHERE "FeaturedImage" LIKE 'https://prnt.sc/%';

-- Update any other image hosting URLs to use Lorem Picsum format
UPDATE "BlogPosts" 
SET "FeaturedImage" = 'https://picsum.photos/id/' || generate_random_code() || '/640/480'
WHERE "FeaturedImage" NOT LIKE 'https://picsum.photos/%' 
  AND "FeaturedImage" IS NOT NULL 
  AND "FeaturedImage" != '';

-- Show updated results
SELECT 
    '=== UPDATED BLOG POST IMAGES ===' AS "Section",
    '' AS "Title",
    '' AS "Author", 
    '' AS "New Image URL"

UNION ALL

SELECT 
    '' AS "Section",
    CASE 
        WHEN LENGTH(bp."Title") > 35 
        THEN LEFT(bp."Title", 32) || '...'
        ELSE bp."Title"
    END AS "Title",
    u."Name" AS "Author",
    bp."FeaturedImage" AS "New Image URL"
FROM "BlogPosts" bp
JOIN "Users" u ON bp."AuthorId" = u."Id"
WHERE bp."FeaturedImage" LIKE 'https://picsum.photos/%'
ORDER BY bp."CreatedAt" DESC
LIMIT 20;

-- Summary statistics
SELECT 
    '=== UPDATE SUMMARY ===' AS "Metric",
    '' AS "Count"

UNION ALL

SELECT 
    'Total Blog Posts Updated:' AS "Metric",
    COUNT(*)::text AS "Count"
FROM "BlogPosts"
WHERE "FeaturedImage" LIKE 'https://picsum.photos/%'

UNION ALL

SELECT 
    'Sample New URLs Generated:' AS "Metric",
    'https://picsum.photos/id/' || generate_random_code() || '/640/480, https://picsum.photos/id/' || generate_random_code() || '/640/480, https://picsum.photos/id/' || generate_random_code() || '/640/480' AS "Count";

-- Verification: Show old vs new format comparison
SELECT 
    '=== URL FORMAT COMPARISON ===' AS "Info",
    '' AS "Details"

UNION ALL

SELECT 
    'Old Format Example:' AS "Info",
    'https://example.com/images/web-dev-basics.jpg' AS "Details"

UNION ALL

SELECT 
    'New Format Example:' AS "Info", 
    'https://picsum.photos/id/' || generate_random_code() || '/640/480' AS "Details"

UNION ALL

SELECT 
    'Pattern:' AS "Info",
    'https://picsum.photos/id/[1-999]/640/480' AS "Details";

-- Clean up the function
DROP FUNCTION generate_random_code();
