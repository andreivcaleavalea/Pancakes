import React, { useEffect, useMemo } from "react";
import { Typography, Spin, Alert, Empty, Row, Col } from "antd";
import { BookOutlined } from "@ant-design/icons";
import { useAuth } from "@/contexts/AuthContext";
import { useRouter } from "@/router/RouterProvider";
import { useSavedBlogsContext } from "@/contexts/SavedBlogsContext";
import { useAverageRatings } from "@/hooks/useAverageRatings";
import { BlogCard } from "@/components/common";
import type { BlogPost } from "@/types/blog";
import "./SavedBlogsPage.scss";

const { Title, Text } = Typography;

// Transform BlogPost to include legacy fields for backward compatibility (same as BlogService)
const transformBlogPost = (post: BlogPost): BlogPost => {
  return {
    ...post,
    description: post.excerpt || post.content.substring(0, 150) + "...",
    date: post.publishedAt || post.createdAt,
    image: post.featuredImage || "/placeholder-image.jpg",
    // Use the author information from the backend (already populated by UserService)
    author: post.authorName || "Unknown Author",
    authorAvatar: post.authorImage || "/default-avatar.png",
  };
};

const SavedBlogsPage: React.FC = () => {
  const { isAuthenticated } = useAuth();
  const { navigate } = useRouter();
  const {
    savedBlogs,
    isLoading: loading,
    refreshSavedBlogs,
  } = useSavedBlogsContext();

  // Transform saved blog posts to include legacy fields
  const transformedSavedBlogs = useMemo(() => {
    return savedBlogs.map((savedBlog) => ({
      ...savedBlog,
      blogPost: savedBlog.blogPost
        ? transformBlogPost(savedBlog.blogPost)
        : undefined,
    }));
  }, [savedBlogs]);

  // Get blog post IDs for average ratings
  const blogPostIds = transformedSavedBlogs
    .filter((sb) => sb.blogPost)
    .map((sb) => sb.blogPost!.id);
  const { averageRatings } = useAverageRatings(blogPostIds);

  // Redirect to login if not authenticated
  useEffect(() => {
    if (!isAuthenticated) {
      navigate("login");
    }
  }, [isAuthenticated, navigate]);

  if (!isAuthenticated) {
    return null; // Will redirect to login
  }

  if (loading) {
    return (
      <div className="saved-blogs-page">
        <div className="saved-blogs-page__header">
          <Title level={2} className="saved-blogs-page__title">
            <BookOutlined /> My Saved Blogs
          </Title>
        </div>
        <div className="saved-blogs-page__loading">
          <Spin size="large" />
          <Text>Loading your saved blogs...</Text>
        </div>
      </div>
    );
  }

  return (
    <div className="saved-blogs-page">
      <div className="container">
        <div className="saved-blogs-page__header">
          <Title level={2} className="saved-blogs-page__title">
            <BookOutlined /> My Saved Blogs
          </Title>
          <Text className="saved-blogs-page__subtitle">
            {transformedSavedBlogs.length} saved{" "}
            {transformedSavedBlogs.length === 1 ? "recipe" : "recipes"}
          </Text>
        </div>

        {transformedSavedBlogs.length === 0 ? (
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description="No saved blogs yet"
            className="saved-blogs-page__empty"
          >
            <Text type="secondary">
              Start saving your favorite recipes by clicking the heart icon on
              any blog post!
            </Text>
          </Empty>
        ) : (
          <div className="saved-blogs-page__content">
            <Row gutter={[12, 24]}>
              {transformedSavedBlogs.map((savedBlog) => {
                if (!savedBlog.blogPost) {
                  return null; // Skip if blog post data is missing
                }

                return (
                  <Col
                    key={savedBlog.blogPostId}
                    xs={24}
                    sm={24}
                    md={12}
                    lg={8}
                    xl={8}
                  >
                    <BlogCard
                      post={savedBlog.blogPost}
                      averageRating={averageRatings[savedBlog.blogPost.id]}
                    />
                  </Col>
                );
              })}
            </Row>
          </div>
        )}
      </div>
    </div>
  );
};

export default SavedBlogsPage;
