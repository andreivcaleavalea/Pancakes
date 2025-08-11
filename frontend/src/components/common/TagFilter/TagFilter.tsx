import React, { useState, useEffect } from "react";
import { Tag, Spin, Alert, Typography } from "antd";
import { tagsApi } from "@/services/tagsApi";
import "./TagFilter.scss";

const { Text } = Typography;

interface TagFilterProps {
  selectedTags: string[];
  onTagsChange: (tags: string[]) => void;
  className?: string;
}

const TagFilter: React.FC<TagFilterProps> = ({
  selectedTags,
  onTagsChange,
  className = "",
}) => {
  const [popularTags, setPopularTags] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchPopularTags = async () => {
      try {
        setLoading(true);
        setError(null);
        const tags = await tagsApi.getPopular(15); // Get top 15 popular tags
        setPopularTags(tags);
      } catch (err) {
        console.error("Error fetching popular tags:", err);
        setError("Failed to load tags");
      } finally {
        setLoading(false);
      }
    };

    fetchPopularTags();
  }, []);

  const handleTagClick = (tag: string) => {
    const isSelected = selectedTags.includes(tag);
    if (isSelected) {
      // Remove tag
      onTagsChange(selectedTags.filter((t) => t !== tag));
    } else {
      // Add tag
      onTagsChange([...selectedTags, tag]);
    }
  };

  const handleClearAll = () => {
    onTagsChange([]);
  };

  if (loading) {
    return (
      <div className={`tag-filter tag-filter--loading ${className}`}>
        <Spin size="small" />
        <Text className="tag-filter__loading-text">Loading tags...</Text>
      </div>
    );
  }

  if (error) {
    return (
      <div className={`tag-filter tag-filter--error ${className}`}>
        <Alert
          message="Failed to load tags"
          type="error"
          size="small"
          showIcon
        />
      </div>
    );
  }

  if (popularTags.length === 0) {
    return null;
  }

  return (
    <div className={`tag-filter ${className}`}>
      <div className="tag-filter__header">
        <Text strong className="tag-filter__title">
          Filter by Tags
        </Text>
        {selectedTags.length > 0 && (
          <button onClick={handleClearAll} className="tag-filter__clear-btn">
            Clear All ({selectedTags.length})
          </button>
        )}
      </div>

      <div className="tag-filter__tags">
        {popularTags.map((tag) => {
          const isSelected = selectedTags.includes(tag);
          return (
            <Tag
              key={tag}
              className={`tag-filter__tag ${
                isSelected ? "tag-filter__tag--selected" : ""
              }`}
              onClick={() => handleTagClick(tag)}
            >
              {tag}
            </Tag>
          );
        })}
      </div>
    </div>
  );
};

export default TagFilter;
