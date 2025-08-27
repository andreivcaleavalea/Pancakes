-- Test Script to Verify Popular Blogs Algorithm Works Correctly
-- Run this after fixing the FriendshipRepository to test friend-based recommendations

-- Create test scenario: user_001 and user_002 are friends
-- user_002 rates and saves a blog from user_001 with specific tags
-- Then check if user_002 gets recommended similar blogs when viewing popular posts

-- 1. Verify friendship exists
SELECT 
    'Friendship Test' AS "Test",
    sender."Name" AS "Sender",
    receiver."Name" AS "Receiver", 
    f."Status",
    CASE f."Status"
        WHEN 1 THEN 'Accepted ✓'
        ELSE 'Not Accepted ✗'
    END AS "Status_Display"
FROM "Friendships" f
JOIN "Users" sender ON f."SenderId" = sender."Id"  
JOIN "Users" receiver ON f."ReceiverId" = receiver."Id"
WHERE (f."SenderId" = 'user_001' AND f."ReceiverId" = 'user_002')
   OR (f."SenderId" = 'user_002' AND f."ReceiverId" = 'user_001');

-- 2. Create a test rating from user_002 to user_001's blog
INSERT INTO "PostRatings" ("Id", "BlogPostId", "UserId", "Rating", "CreatedAt", "UpdatedAt")
SELECT 
    gen_random_uuid(),
    bp."Id",
    'user_002',
    4.5,
    NOW(),
    NOW()
FROM "BlogPosts" bp
WHERE bp."AuthorId" = 'user_001' 
  AND bp."Status" = 1 -- Published
  AND NOT EXISTS (
      SELECT 1 FROM "PostRatings" pr 
      WHERE pr."BlogPostId" = bp."Id" AND pr."UserId" = 'user_002'
  )
LIMIT 1;

-- 3. Create a test saved blog from user_002 to user_001's blog  
INSERT INTO "SavedBlogs" ("UserId", "BlogPostId", "SavedAt")
SELECT 
    'user_002',
    bp."Id", 
    NOW()
FROM "BlogPosts" bp
WHERE bp."AuthorId" = 'user_001'
  AND bp."Status" = 1 -- Published
  AND NOT EXISTS (
      SELECT 1 FROM "SavedBlogs" sb 
      WHERE sb."BlogPostId" = bp."Id" AND sb."UserId" = 'user_002'
  )
LIMIT 1;

-- 4. Verify the test data was created
SELECT 
    '=== TEST DATA VERIFICATION ===' AS "Section",
    '' AS "Details";

SELECT 
    'Ratings by user_002 on user_001 blogs:' AS "Test",
    COUNT(*)::text AS "Count"
FROM "PostRatings" pr
JOIN "BlogPosts" bp ON pr."BlogPostId" = bp."Id"
WHERE pr."UserId" = 'user_002' AND bp."AuthorId" = 'user_001';

SELECT 
    'Blogs saved by user_002 from user_001:' AS "Test", 
    COUNT(*)::text AS "Count"
FROM "SavedBlogs" sb
JOIN "BlogPosts" bp ON sb."BlogPostId" = bp."Id"
WHERE sb."UserId" = 'user_002' AND bp."AuthorId" = 'user_001';

-- 5. Show what tags user_002 should prefer based on interactions
SELECT 
    '=== PREFERRED TAGS FOR user_002 ===' AS "Section",
    '' AS "Tag"

UNION ALL

SELECT 
    'From Saved Blogs:' AS "Section",
    UNNEST(bp."Tags") AS "Tag"
FROM "SavedBlogs" sb
JOIN "BlogPosts" bp ON sb."BlogPostId" = bp."Id"
WHERE sb."UserId" = 'user_002'

UNION ALL 

SELECT 
    'From Highly Rated Blogs (4+):' AS "Section",
    UNNEST(bp."Tags") AS "Tag"
FROM "PostRatings" pr
JOIN "BlogPosts" bp ON pr."BlogPostId" = bp."Id"
WHERE pr."UserId" = 'user_002' AND pr."Rating" >= 4.0;

-- 6. Check algorithm inputs for user_002
SELECT 
    '=== ALGORITHM INPUTS FOR user_002 ===' AS "Section",
    '' AS "Value"

UNION ALL

SELECT 
    'Total Published Posts:' AS "Section",
    COUNT(*)::text AS "Value"
FROM "BlogPosts" 
WHERE "Status" = 1

UNION ALL

SELECT 
    'Friends Count:' AS "Section", 
    COUNT(DISTINCT 
        CASE WHEN f."SenderId" = 'user_002' THEN f."ReceiverId"
             WHEN f."ReceiverId" = 'user_002' THEN f."SenderId"
        END
    )::text AS "Value"
FROM "Friendships" f
WHERE (f."SenderId" = 'user_002' OR f."ReceiverId" = 'user_002')
  AND f."Status" = 1 -- Accepted

UNION ALL

SELECT 
    'User Ratings Count:' AS "Section",
    COUNT(*)::text AS "Value"  
FROM "PostRatings"
WHERE "UserId" = 'user_002'

UNION ALL

SELECT 
    'User Saved Posts Count:' AS "Section",
    COUNT(*)::text AS "Value"
FROM "SavedBlogs"
WHERE "UserId" = 'user_002';

-- 7. Show expected algorithm behavior
SELECT 
    '=== EXPECTED ALGORITHM BEHAVIOR ===' AS "Info",
    '' AS "Details"

UNION ALL

SELECT 
    'Algorithm Threshold:' AS "Info",
    'Minimum 5 posts required ✓' AS "Details"

UNION ALL

SELECT 
    'Friendship Weight:' AS "Info", 
    '25% - Posts rated/saved by friends get bonus' AS "Details"

UNION ALL

SELECT 
    'Tag Preference Weight:' AS "Info",
    '20% - Posts with similar tags get bonus' AS "Details"

UNION ALL

SELECT 
    'Expected Result:' AS "Info",
    'user_002 should see posts with similar tags to rated/saved content' AS "Details";

-- 8. Show candidate posts for recommendations (excluding already interacted)
SELECT 
    '=== CANDIDATE POSTS FOR RECOMMENDATIONS ===' AS "Section",
    '' AS "Title",
    '' AS "Author", 
    '' AS "Tags",
    '' AS "Reason"

UNION ALL

SELECT 
    '' AS "Section",
    CASE 
        WHEN LENGTH(bp."Title") > 40 
        THEN LEFT(bp."Title", 37) || '...'
        ELSE bp."Title"
    END AS "Title",
    u."Name" AS "Author",
    bp."Tags"::text AS "Tags",
    CASE 
        WHEN bp."AuthorId" IN (
            SELECT CASE WHEN f."SenderId" = 'user_002' THEN f."ReceiverId"
                       WHEN f."ReceiverId" = 'user_002' THEN f."SenderId"
                  END
            FROM "Friendships" f
            WHERE (f."SenderId" = 'user_002' OR f."ReceiverId" = 'user_002')
              AND f."Status" = 1
        ) THEN 'Friend''s Post ⭐'
        WHEN EXISTS (
            SELECT 1 FROM "SavedBlogs" sb2 
            JOIN "BlogPosts" bp2 ON sb2."BlogPostId" = bp2."Id"
            WHERE sb2."UserId" = 'user_002' 
              AND bp2."Tags" && bp."Tags" -- Tag overlap
        ) THEN 'Similar Tags 🏷️'
        ELSE 'General'
    END AS "Reason"
FROM "BlogPosts" bp
JOIN "Users" u ON bp."AuthorId" = u."Id"
WHERE bp."Status" = 1 -- Published
  AND bp."AuthorId" != 'user_002' -- Not own posts
  AND bp."Id" NOT IN (
      SELECT "BlogPostId" FROM "SavedBlogs" WHERE "UserId" = 'user_002'
  )
  AND bp."Id" NOT IN (
      SELECT "BlogPostId" FROM "PostRatings" WHERE "UserId" = 'user_002' 
  )
ORDER BY 
    CASE 
        WHEN bp."AuthorId" IN (
            SELECT CASE WHEN f."SenderId" = 'user_002' THEN f."ReceiverId"
                       WHEN f."ReceiverId" = 'user_002' THEN f."SenderId"
                  END
            FROM "Friendships" f
            WHERE (f."SenderId" = 'user_002' OR f."ReceiverId" = 'user_002')
              AND f."Status" = 1
        ) THEN 1
        WHEN EXISTS (
            SELECT 1 FROM "SavedBlogs" sb2 
            JOIN "BlogPosts" bp2 ON sb2."BlogPostId" = bp2."Id"
            WHERE sb2."UserId" = 'user_002' 
              AND bp2."Tags" && bp."Tags"
        ) THEN 2  
        ELSE 3
    END,
    bp."CreatedAt" DESC
LIMIT 10;

