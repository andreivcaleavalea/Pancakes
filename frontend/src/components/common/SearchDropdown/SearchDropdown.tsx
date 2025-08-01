import React, { useState, useEffect, useRef } from "react";
import { Input } from "antd";
import { SearchOutlined } from "@ant-design/icons";
import { BlogService } from "../../../services/blogService";
import { useRouter } from "../../../router/RouterProvider";
import type { BlogPost } from "../../../types/blog";
import "./SearchDropdown.scss";

const SearchDropdown: React.FC = () => {
  const [searchValue, setSearchValue] = useState("");
  const [searchResults, setSearchResults] = useState<BlogPost[]>([]);
  const [showDropdown, setShowDropdown] = useState(false);
  const { navigate } = useRouter();
  const searchRef = useRef<HTMLDivElement>(null);

  // Debounce search
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      if (searchValue.trim() && searchValue.length >= 2) {
        performSearch(searchValue.trim());
      } else {
        setSearchResults([]);
        setShowDropdown(false);
      }
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [searchValue]);

  // Handle clicks outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        searchRef.current &&
        !searchRef.current.contains(event.target as Node)
      ) {
        setShowDropdown(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const performSearch = async (query: string) => {
    try {
      const result = await BlogService.searchPosts(query, 1, 5); // Limit to 5 results
      setSearchResults(result.data);
      setShowDropdown(result.data.length > 0);
    } catch (error) {
      console.error("Search error:", error);
      setSearchResults([]);
      setShowDropdown(false);
    }
  };

  const handleResultClick = (post: BlogPost) => {
    navigate("blog-view", undefined, post.id);
    setSearchValue("");
    setShowDropdown(false);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchValue(e.target.value);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Escape") {
      setShowDropdown(false);
    }
  };

  return (
    <div className="search-dropdown" ref={searchRef}>
      <Input
        placeholder="Search..."
        prefix={<SearchOutlined />}
        className="search-dropdown__input"
        size="middle"
        value={searchValue}
        onChange={handleInputChange}
        onKeyDown={handleKeyDown}
        onFocus={() => {
          if (searchResults.length > 0) {
            setShowDropdown(true);
          }
        }}
      />

      {showDropdown && (
        <div className="search-dropdown__dropdown">
          {searchResults.length > 0 ? (
            <div className="search-dropdown__results">
              {searchResults.map((post) => (
                <div
                  key={post.id}
                  className="search-dropdown__result"
                  onClick={() => handleResultClick(post)}
                >
                  <div className="search-dropdown__result-title">
                    {post.title}
                  </div>
                  <div className="search-dropdown__result-excerpt">
                    {post.excerpt || post.content?.substring(0, 100) + "..."}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="search-dropdown__no-results">No results found</div>
          )}
        </div>
      )}
    </div>
  );
};

export default SearchDropdown;
