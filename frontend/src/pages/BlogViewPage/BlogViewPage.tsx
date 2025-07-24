import React, { useState } from "react";
import { Typography, Spin, Alert, Button, Space, message, App } from "antd";
import {
  ArrowLeftOutlined,
  HeartOutlined,
  HeartFilled,
  EditOutlined,
  DeleteOutlined,
} from "@ant-design/icons";
import { useRouter } from "@/router/RouterProvider";
import { useAuth } from "@/contexts/AuthContext";
import { useBlogPost, useFavorite } from "@/hooks/useBlog";
import { usePostRating } from "@/hooks/useRating";
import { CommentSection, GlazeMeter, CachedAvatar } from "@/components/common";
import { blogPostsApi } from "@/services/blogApi";
import { DEFAULTS } from "@/utils/constants";
import "./BlogViewPage.scss";

const { Title, Text, Paragraph } = Typography;

const BlogViewPage: React.FC = () => {
  const { blogId, navigate } = useRouter();
  const { user, isAuthenticated } = useAuth();
  const [deleteLoading, setDeleteLoading] = useState(false);
  const { modal } = App.useApp();

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

  const handleEdit = () => {
    if (blogId) {
      navigate("edit-blog", undefined, blogId);
    }
  };

  const handleDelete = () => {
    if (!blogId) {
      return;
    }

    modal.confirm({
      title: "Delete Blog Post",
      content:
        "Are you sure you want to delete this blog post? This action cannot be undone.",
      okText: "Delete",
      okType: "danger",
      cancelText: "Cancel",
      onOk: async () => {
        try {
          setDeleteLoading(true);
          await blogPostsApi.delete(blogId);
          message.success("Blog post deleted successfully!");
          navigate("home");
        } catch (error) {
          console.error("Error deleting blog post:", error);
          message.error("Failed to delete blog post. Please try again.");
        } finally {
          setDeleteLoading(false);
        }
      },
    });
  };

  // Check if current user is the author of the blog post
  const isAuthor = isAuthenticated && user && blog && blog.authorId === user.id;

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
            <CachedAvatar
              src={blogWithDisplay.authorAvatar}
              fallbackSrc={DEFAULTS.AVATAR}
              size={48}
              className="blog-view__author-avatar"
            >
              {blogWithDisplay.author.charAt(0)}
            </CachedAvatar>
            <div className="blog-view__author-info">
              <Text strong className="blog-view__author-name">
                By {blogWithDisplay.author}
              </Text>
              <Text className="blog-view__date">{blogWithDisplay.date}</Text>
            </div>
          </div>

          <div className="blog-view__actions">
            {isAuthor ? (
              <Space size="middle">
                <Button
                  type="default"
                  icon={<EditOutlined />}
                  onClick={handleEdit}
                  className="blog-view__edit-button"
                >
                  Edit
                </Button>
                <Button
                  type="default"
                  danger
                  icon={<DeleteOutlined />}
                  onClick={handleDelete}
                  loading={deleteLoading}
                  className="blog-view__delete-button"
                >
                  Delete
                </Button>
              </Space>
            ) : (
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
            )}
          </div>
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
            readonly={!isAuthenticated}
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
