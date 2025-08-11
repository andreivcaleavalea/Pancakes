import React from "react";
import { Tag } from "antd";
import "./BlogTags.scss";

interface BlogTagsProps {
  tags: string[];
  maxVisible?: number;
  size?: "small" | "default";
  className?: string;
  color?: string;
}

const BlogTags: React.FC<BlogTagsProps> = ({
  tags,
  maxVisible = 3,
  size = "small",
  className = "",
  color = "default",
}) => {
  if (!tags || tags.length === 0) {
    return null;
  }

  const visibleTags = tags.slice(0, maxVisible);
  const remainingCount = tags.length - maxVisible;

  return (
    <div className={`blog-tags ${className}`}>
      {visibleTags.map((tag, index) => (
        <Tag
          key={index}
          color={color}
          className={`blog-tags__tag blog-tags__tag--${size}`}
        >
          {tag}
        </Tag>
      ))}
      {remainingCount > 0 && (
        <Tag
          color="default"
          className={`blog-tags__tag blog-tags__tag--more blog-tags__tag--${size}`}
        >
          +{remainingCount}
        </Tag>
      )}
    </div>
  );
};

export default BlogTags;
