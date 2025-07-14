import React, { useState } from 'react';
import { Card, Typography, App } from 'antd';
import { HeartOutlined, HeartFilled } from '@ant-design/icons';
import type { BlogPost } from '@/types/blog';
import { toggleFavorite } from './api';
import './BlogCard.scss';

const { Title, Text, Paragraph } = Typography;

interface BlogCardProps {
  post: BlogPost;
  variant?: 'default' | 'horizontal' | 'featured';
}

const BlogCard: React.FC<BlogCardProps> = ({ post, variant = 'default' }) => {
  const [isFavorite, setIsFavorite] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  
  const { message } = App.useApp();

  const handleFavoriteClick = async (e: React.MouseEvent) => {
    e.stopPropagation();

    setIsLoading(true);
    try {
      const newFavoriteStatus = !isFavorite;
      const success = await toggleFavorite(post.id, newFavoriteStatus);

      if (success) {
        setIsFavorite(newFavoriteStatus);
        message.success(
          newFavoriteStatus ? 'Added to favorites!' : 'Removed from favorites!'
        );
      } else {
        message.error('Failed to update favorite status');
      }
    } catch (error) {
      console.error('Error toggling favorite:', error);
      message.error('Failed to update favorite status');
    } finally {
      setIsLoading(false);
    }
  };
  if (variant === 'horizontal') {
    return (
      <div className="blog-card blog-card--horizontal">
        <img src={post.image} alt={post.title} className="blog-card__image--horizontal" />
        <div className="blog-card__content">
          <div className="blog-card__meta">
            <Text className="blog-card__date">{post.date}</Text>
            <div className="blog-card__header">
              <Title level={4} className="blog-card__title">{post.title}</Title>
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
            <Paragraph className="blog-card__description">{post.description}</Paragraph>
          </div>
        </div>
      </div>
    );
  }

  return (
    <Card className={`blog-card ${variant === 'featured' ? 'blog-card--featured' : ''}`} variant="borderless">
      <img src={post.image} alt={post.title} className="blog-card__image" />
      <div className="blog-card__content">
        <div className="blog-card__meta">
          <Text className="blog-card__date">{post.date}</Text>
          <div className="blog-card__header">
            <Title level={variant === 'featured' ? 2 : 4} className="blog-card__title">
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
            <Text className="blog-card__author">From {post.author}</Text>
          )}
          {post.description && (
            <Paragraph className="blog-card__description">{post.description}</Paragraph>
          )}
        </div>
      </div>
    </Card>
  );
};

export default BlogCard; 