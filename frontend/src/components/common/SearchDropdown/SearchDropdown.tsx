import React, { useState, useEffect, useRef } from "react";
import { Input, Spin, Empty } from "antd";
import { SearchOutlined } from "@ant-design/icons";
import { useRouter } from "../../../router/RouterProvider";
import { blogPostsApi } from "../../../services/blogApi";
import type { BlogPost } from "../../../types/blog";
import "./SearchDropdown.scss";

interface SearchDropdownProps {
  placeholder?: string;
  className?: string;
  size?: "small" | "middle" | "large";
}

const SearchDropdown: React.FC<SearchDropdownProps> = ({
  placeholder = "Search blogs...",
  className = "",
  size = "middle",
}) => {
  const [searchTerm, setSearchTerm] = useState("");
  const [results, setResults] = useState<BlogPost[]>([]);
  const [loading, setLoading] = useState(false);
  const [showDropdown, setShowDropdown] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(-1);

  const searchRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<any>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const { navigate } = useRouter();

  // Debounced search function
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      if (searchTerm.trim().length > 0) {
        performSearch(searchTerm.trim());
      } else {
        setResults([]);
        setShowDropdown(false);
      }
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [searchTerm]);

  // Click outside handler
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        searchRef.current &&
        !searchRef.current.contains(event.target as Node)
      ) {
        setShowDropdown(false);
        setSelectedIndex(-1);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const performSearch = async (term: string) => {
    setLoading(true);
    try {
      const response = await blogPostsApi.getAll({
        search: term,
        pageSize: 10, // Get 10 results, show 5 with scroll
        page: 1,
        status: 1, // Only published posts
      });
      setResults(response.data);
      setShowDropdown(true);
      setSelectedIndex(-1);
    } catch (error) {
      console.error("Search error:", error);
      setResults([]);
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setSearchTerm(value);
    if (value.trim().length === 0) {
      setShowDropdown(false);
      setSelectedIndex(-1);
    }
  };

  const handleInputFocus = () => {
    if (searchTerm.trim().length > 0 && results.length > 0) {
      setShowDropdown(true);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (!showDropdown || results.length === 0) return;

    switch (e.key) {
      case "ArrowDown":
        e.preventDefault();
        setSelectedIndex((prev) =>
          prev < results.length - 1 ? prev + 1 : prev
        );
        break;
      case "ArrowUp":
        e.preventDefault();
        setSelectedIndex((prev) => (prev > 0 ? prev - 1 : -1));
        break;
      case "Enter":
        e.preventDefault();
        if (selectedIndex >= 0 && selectedIndex < results.length) {
          handleResultClick(results[selectedIndex]);
        }
        break;
      case "Escape":
        setShowDropdown(false);
        setSelectedIndex(-1);
        inputRef.current?.blur();
        break;
    }
  };

  const handleResultClick = (post: BlogPost) => {
    setShowDropdown(false);
    setSearchTerm("");
    setSelectedIndex(-1);
    navigate("blog-view", undefined, post.id);
  };

  const highlightSearchTerm = (text: string, term: string) => {
    // Just return the original text without highlighting to avoid spacing issues
    return text;
  };

  return (
    <div className={`search-dropdown ${className}`} ref={searchRef}>
      <Input
        ref={inputRef}
        placeholder={placeholder}
        prefix={<SearchOutlined />}
        value={searchTerm}
        onChange={handleInputChange}
        onFocus={handleInputFocus}
        onKeyDown={handleKeyDown}
        size={size}
        className="search-dropdown__input"
        suffix={loading ? <Spin size="small" /> : null}
      />

      {showDropdown && (
        <div className="search-dropdown__dropdown" ref={dropdownRef}>
          {loading ? (
            <div className="search-dropdown__loading">
              <Spin size="small" />
              <span>Searching...</span>
            </div>
          ) : results.length > 0 ? (
            <div className="search-dropdown__results">
              {results.map((post, index) => (
                <div
                  key={post.id}
                  className={`search-dropdown__result ${
                    index === selectedIndex
                      ? "search-dropdown__result--selected"
                      : ""
                  }`}
                  onClick={() => handleResultClick(post)}
                >
                  <div className="search-dropdown__result-content">
                    <h4 className="search-dropdown__result-title">
                      {highlightSearchTerm(post.title, searchTerm)}
                    </h4>
                    <p className="search-dropdown__result-author">
                      by {post.authorName}
                    </p>
                    {post.excerpt && (
                      <p className="search-dropdown__result-excerpt">
                        {highlightSearchTerm(post.excerpt, searchTerm)}
                      </p>
                    )}
                  </div>
                  {post.featuredImage && (
                    <div className="search-dropdown__result-image">
                      <img src={post.featuredImage} alt={post.title} />
                    </div>
                  )}
                </div>
              ))}
            </div>
          ) : searchTerm.trim().length > 0 ? (
            <div className="search-dropdown__empty">
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description="No blogs found"
              />
            </div>
          ) : null}
        </div>
      )}
    </div>
  );
};

export default SearchDropdown;
