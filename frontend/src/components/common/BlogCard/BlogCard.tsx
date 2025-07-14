import React from 'react';
import { Card, Tag, Typography } from 'antd';
import { HeartOutlined } from '@ant-design/icons';
import type { BlogPost } from '@/types/blog';
import './BlogCard.scss';

const { Title, Text, Paragraph } = Typography;

interface BlogCardProps {
  post: BlogPost;
  variant?: 'default' | 'horizontal' | 'featured';
}

const BlogCard: React.FC<BlogCardProps> = ({ post, variant = 'default' }) => {
  const handleTagClick = (tagName: string) => {
    console.log(`Tag clicked: ${tagName}`);
  };

  const renderTags = () => (
    <div className="blog-card__tags">
      {post.tags.map((tag: any, index: number) => (
        <Tag
          key={index}
          className="blog-card__tag"
          style={{
            backgroundColor: tag.backgroundColor,
            color: tag.color
          }}
          onClick={() => handleTagClick(tag.name)}
        >
          {tag.name}
        </Tag>
      ))}
    </div>
  );

  if (variant === 'horizontal') {
    return (
      <div className="blog-card blog-card--horizontal">
        <img src={post.image} alt={post.title} className="blog-card__image--horizontal" />
        <div className="blog-card__content">
          <div className="blog-card__meta">
            <Text className="blog-card__date">{post.date}</Text>
            <div className="blog-card__header">
              <Title level={4} className="blog-card__title">{post.title}</Title>
            </div>
            <Paragraph className="blog-card__description">{post.description}</Paragraph>
          </div>
          {renderTags()}
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
            <HeartOutlined className="blog-card__favorite" />
          </div>
          {post.author && (
            <Text className="blog-card__author">From {post.author}</Text>
          )}
          {post.description && (
            <Paragraph className="blog-card__description">{post.description}</Paragraph>
          )}
        </div>
        {renderTags()}
      </div>
    </Card>
  );
};

export default BlogCard; 