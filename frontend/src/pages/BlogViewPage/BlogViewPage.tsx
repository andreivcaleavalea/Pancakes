import React from "react";
import { Typography, Avatar, Spin, Alert, Button } from "antd";
import {
  ArrowLeftOutlined,
  HeartOutlined,
  HeartFilled,
} from "@ant-design/icons";
import { useRouter } from "@/router/RouterProvider";
import { useBlogPost, useFavorite } from "@/hooks/useBlog";
import { usePostRating } from "@/hooks/useRating";
import { CommentSection, GlazeMeter } from "@/components/common";
import { DEFAULTS } from "@/utils/constants";
import "./BlogViewPage.scss";

const { Title, Text, Paragraph } = Typography;

const BlogViewPage: React.FC = () => {
  const { blogId, navigate } = useRouter();

  // Use the real data hooks
  const { data: blog, loading, error } = useBlogPost(blogId);

  const {
    isFavorite,
    loading: favoriteLoading,
    toggleFavorite,
  } = useFavorite(blogId || "", blog?.isFeatured || false);
  const { stats: ratingStats, submitRating } = usePostRating(blogId || "");

  const handleBack = () => {
    navigate("home");
  };

  // Increment view count when blog loads (optional - will fail gracefully if not implemented)
  React.useEffect(() => {
    if (blog && blogId) {
      import("@/services/blogApi").then(({ blogPostsApi }) => {
        blogPostsApi.incrementViewCount(blogId).catch(() => {
          // Silently fail if view count endpoint is not implemented
        });
      });
    }
  }, [blog, blogId]);

  const handleFavoriteToggle = async () => {
    await toggleFavorite();
  };

  if (loading) {
    return (
      <div className="blog-view__loading">
        <Spin size="large" />
      </div>
    );
  }

  if (error || !blog) {
    return (
      <div className="blog-view__error">
        <Alert
          message="Error Loading Blog Post"
          description={error || "Blog post not found"}
          type="error"
          showIcon
          action={
            <Button size="small" onClick={handleBack}>
              Go Back
            </Button>
          }
        />
      </div>
    );
  }

  // Transform blog data to include display fields
  const blogWithDisplay = {
    ...blog,
    image: (blog as any).image || blog.featuredImage || DEFAULTS.IMAGE,
    author: (blog as any).author || "Unknown Author",
    authorAvatar: (blog as any).authorAvatar || DEFAULTS.AVATAR,
    date:
      (blog as any).date ||
      new Date(blog.publishedAt || blog.createdAt).toLocaleDateString(),
    description:
      (blog as any).description ||
      blog.excerpt ||
      blog.content.substring(0, 150) + "...",
  };

  return (
    <div className="blog-view">
      <div className="blog-view__header">
        <Button
          type="text"
          icon={<ArrowLeftOutlined />}
          onClick={handleBack}
          className="blog-view__back-button"
        >
          Back to Home
        </Button>
      </div>

      <article className="blog-view__article">
        <div className="blog-view__hero">
          <img
            src={blogWithDisplay.image}
            alt={blog.title}
            className="blog-view__featured-image"
          />
          <div className="blog-view__hero-overlay">
            <div className="blog-view__hero-content">
              <Title level={1} className="blog-view__title">
                {blog.title}
              </Title>
            </div>
          </div>
        </div>

        <div className="blog-view__meta">
          <div className="blog-view__author">
            <Avatar
              src={blogWithDisplay.authorAvatar}
              size={48}
              className="blog-view__author-avatar"
            >
              {blogWithDisplay.author.charAt(0)}
            </Avatar>
            <div className="blog-view__author-info">
              <Text strong className="blog-view__author-name">
                By {blogWithDisplay.author}
              </Text>
              <Text className="blog-view__date">{blogWithDisplay.date}</Text>
            </div>
          </div>

          <Button
            type="text"
            icon={isFavorite ? <HeartFilled /> : <HeartOutlined />}
            onClick={handleFavoriteToggle}
            loading={favoriteLoading}
            className={`blog-view__favorite ${
              isFavorite ? "blog-view__favorite--active" : ""
            }`}
          >
            {isFavorite ? "Favorited" : "Add to Favorites"}
          </Button>
        </div>

        <div className="blog-view__content">
          <div dangerouslySetInnerHTML={{ __html: blog.content }} />
        </div>

        {/* Glaze Meter Rating System */}
        <div className="blog-view__rating">
          <GlazeMeter
            blogPostId={blogId || ""}
            averageRating={ratingStats?.averageRating || 0}
            totalRatings={ratingStats?.totalRatings || 0}
            userRating={ratingStats?.userRating}
            onRate={submitRating}
          />
        </div>

        <div className="blog-view__stats">
          <Text className="blog-view__view-count">
            {(blog as any).viewCount || 0} views
          </Text>
        </div>

        {/* Comment Section */}
        <div className="blog-view__comments">
          <CommentSection blogPostId={blog.id} />
        </div>
      </article>
    </div>
  );
};

export default BlogViewPage;
