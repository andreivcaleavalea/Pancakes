import React from "react";
import { Card, Typography, App, Avatar } from "antd";
import { HeartOutlined, HeartFilled } from "@ant-design/icons";
import type { BlogPost } from "@/types/blog";

interface BlogPostWithDisplay extends BlogPost {
  image?: string;
  date?: string;
  description?: string;
  author?: string;
  authorAvatar?: string;
}
import { useFavorite } from "@/hooks/useBlog";
import { useRouter } from "@/router/RouterProvider";
import { SUCCESS_MESSAGES, ERROR_MESSAGES, DEFAULTS } from "@/utils/constants";
import { AverageRatingDisplay } from "@/components/common";
import "./BlogCard.scss";

const { Title, Text, Paragraph } = Typography;

interface BlogCardProps {
  post: BlogPostWithDisplay;
  variant?: "default" | "horizontal" | "featured";
  averageRating?: { averageRating: number; totalRatings: number };
}

const BlogCard: React.FC<BlogCardProps> = ({
  post,
  variant = "default",
  averageRating,
}) => {
  const {
    isFavorite,
    loading: isLoading,
    toggleFavorite,
  } = useFavorite(post.id, post.isFeatured);
  const { message } = App.useApp();
  const { navigate } = useRouter();

  const handleFavoriteClick = async (e: React.MouseEvent) => {
    e.stopPropagation();

    const success = await toggleFavorite();

    if (success) {
      message.success(
        !isFavorite
          ? SUCCESS_MESSAGES.FAVORITE_ADDED
          : SUCCESS_MESSAGES.FAVORITE_REMOVED
      );
    } else {
      message.error(ERROR_MESSAGES.FAVORITE_ERROR);
    }
  };

  const handleCardClick = () => {
    navigate("blog-view", undefined, post.id);
  };
  if (variant === "horizontal") {
    return (
      <div
        className="blog-card blog-card--horizontal"
        onClick={handleCardClick}
        style={{ cursor: "pointer" }}
      >
        <img
          src={post.image || DEFAULTS.IMAGE}
          alt={post.title}
          className="blog-card__image--horizontal"
        />
        <div className="blog-card__content">
          <div className="blog-card__meta">
            <Text className="blog-card__date">{post.date}</Text>
            <div className="blog-card__header">
              <Title level={4} className="blog-card__title">
                {post.title}
              </Title>
              {isFavorite ? (
                <HeartFilled
                  className="blog-card__favorite blog-card__favorite--active"
                  onClick={handleFavoriteClick}
                  style={{ opacity: isLoading ? 0.5 : 1 }}
                />
              ) : (
                <HeartOutlined
                  className="blog-card__favorite"
                  onClick={handleFavoriteClick}
                  style={{ opacity: isLoading ? 0.5 : 1 }}
                />
              )}
            </div>
            <Paragraph className="blog-card__description">
              {post.description}
            </Paragraph>
            {post.author && (
              <div className="blog-card__author-info">
                <Avatar
                  src={post.authorAvatar || DEFAULTS.AVATAR}
                  size={32}
                  style={{ marginRight: 8 }}
                >
                  {post.author.charAt(0)}
                </Avatar>
                <Text className="blog-card__author">By {post.author}</Text>
              </div>
            )}
            {averageRating && (
              <div className="blog-card__user-rating">
                <AverageRatingDisplay
                  averageRating={averageRating.averageRating}
                  totalRatings={averageRating.totalRatings}
                  size="small"
                />
              </div>
            )}
          </div>
        </div>
      </div>
    );
  }

  return (
    <Card
      className={`blog-card ${
        variant === "featured" ? "blog-card--featured" : ""
      }`}
      variant="borderless"
      onClick={handleCardClick}
      style={{ cursor: "pointer" }}
    >
      <img src={post.image} alt={post.title} className="blog-card__image" />
      <div className="blog-card__content">
        <div className="blog-card__meta">
          <Text className="blog-card__date">{post.date}</Text>
          <div className="blog-card__header">
            <Title
              level={variant === "featured" ? 2 : 4}
              className="blog-card__title"
            >
              {post.title}
            </Title>
            {isFavorite ? (
              <HeartFilled
                className="blog-card__favorite blog-card__favorite--active"
                onClick={handleFavoriteClick}
                style={{ opacity: isLoading ? 0.5 : 1 }}
              />
            ) : (
              <HeartOutlined
                className="blog-card__favorite"
                onClick={handleFavoriteClick}
                style={{ opacity: isLoading ? 0.5 : 1 }}
              />
            )}
          </div>
          {post.author && (
            <div className="blog-card__author-info">
              <Avatar
                src={post.authorAvatar}
                size={variant === "featured" ? 40 : 32}
                style={{ marginRight: 8 }}
              >
                {post.author.charAt(0)}
              </Avatar>
              <Text className="blog-card__author">By {post.author}</Text>
            </div>
          )}
          {post.description && (
            <Paragraph className="blog-card__description">
              {post.description}
            </Paragraph>
          )}
          {averageRating && (
            <div className="blog-card__user-rating">
              <AverageRatingDisplay
                averageRating={averageRating.averageRating}
                totalRatings={averageRating.totalRatings}
                size="small"
              />
            </div>
          )}
        </div>
      </div>
    </Card>
  );
};

export default BlogCard;
