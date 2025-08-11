import React, { useState, useEffect } from "react";
import { Select, Tag, message } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import "./TagInput.scss";

interface TagInputProps {
  value?: string[];
  onChange?: (tags: string[]) => void;
  placeholder?: string;
  maxTags?: number;
  disabled?: boolean;
  className?: string;
}

const TagInput: React.FC<TagInputProps> = ({
  value = [],
  onChange,
  placeholder = "Add tags...",
  maxTags = 10,
  disabled = false,
  className = "",
}) => {
  const [inputValue, setInputValue] = useState("");
  const [suggestions, setSuggestions] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);

  // Fetch tag suggestions as user types
  useEffect(() => {
    const fetchSuggestions = async () => {
      if (inputValue.length >= 2) {
        setLoading(true);
        try {
          // Use real API to search for existing tags
          const { tagsApi } = await import("@/services/tagsApi");
          const results = await tagsApi.search(inputValue, 10);

          // Filter out tags that are already selected
          const filteredResults = results.filter((tag) => !value.includes(tag));
          setSuggestions(filteredResults);
        } catch (error) {
          console.error("Error fetching tag suggestions:", error);
          // Fallback to empty suggestions on error
          setSuggestions([]);
        } finally {
          setLoading(false);
        }
      } else {
        setSuggestions([]);
      }
    };

    const timeoutId = setTimeout(fetchSuggestions, 300);
    return () => clearTimeout(timeoutId);
  }, [inputValue, value]);

  const handleSearch = (searchValue: string) => {
    setInputValue(searchValue);
  };

  const handleSelect = (selectedTag: string) => {
    if (selectedTag && !value.includes(selectedTag)) {
      if (value.length >= maxTags) {
        message.warning(`Maximum ${maxTags} tags allowed`);
        return;
      }

      const newTags = [...value, selectedTag];
      onChange?.(newTags);
      setInputValue("");
      setSuggestions([]);
    }
  };

  const handleDeselect = (removedTag: string) => {
    const newTags = value.filter((tag) => tag !== removedTag);
    onChange?.(newTags);
  };

  const handleInputKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && inputValue.trim()) {
      e.preventDefault();
      const newTag = inputValue.trim();

      if (value.includes(newTag)) {
        message.warning("Tag already exists");
        return;
      }

      if (value.length >= maxTags) {
        message.warning(`Maximum ${maxTags} tags allowed`);
        return;
      }

      handleSelect(newTag);
    }
  };

  const options = suggestions.map((tag) => ({
    label: tag,
    value: tag,
  }));

  // Add current input as an option if it's not empty and not already selected
  if (
    inputValue.trim() &&
    !value.includes(inputValue.trim()) &&
    !suggestions.includes(inputValue.trim())
  ) {
    options.unshift({
      label: (
        <div style={{ display: "flex", alignItems: "center" }}>
          <PlusOutlined style={{ marginRight: 8, color: "#1890ff" }} />
          Create "{inputValue.trim()}"
        </div>
      ),
      value: inputValue.trim(),
    });
  }

  return (
    <div className={`tag-input ${className}`}>
      <div className="tag-input__selected">
        {value.map((tag, index) => (
          <Tag
            key={index}
            closable={!disabled}
            onClose={() => handleDeselect(tag)}
            className="tag-input__selected-tag"
          >
            {tag}
          </Tag>
        ))}
      </div>

      {!disabled && value.length < maxTags && (
        <Select
          mode="combobox"
          placeholder={placeholder}
          value={inputValue}
          options={options}
          onSearch={handleSearch}
          onSelect={handleSelect}
          onInputKeyDown={handleInputKeyDown}
          loading={loading}
          className="tag-input__select"
          dropdownClassName="tag-input__dropdown"
          notFoundContent={
            inputValue.length >= 2 ? "No matching tags" : "Type to search tags"
          }
          showSearch
          filterOption={false}
          allowClear
        />
      )}

      <div className="tag-input__info">
        {value.length}/{maxTags} tags
      </div>
    </div>
  );
};

export default TagInput;
