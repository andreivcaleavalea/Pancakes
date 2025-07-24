/**
 * Application Constants
 * Centralized configuration and constants for the blog application
 */

// API Configuration
export const API_CONFIG = {
  BLOG_API_URL: import.meta.env.VITE_BLOG_API_URL || "http://localhost:5001",
  USER_API_URL: import.meta.env.VITE_USER_API_URL || "http://localhost:5141",
  TIMEOUT: 10000, // 10 seconds
} as const;

// Pagination Configuration
export const PAGINATION = {
  HOME_PAGE_FEATURED: 3,
  HOME_PAGE_HORIZONTAL: 4,
  HOME_PAGE_GRID: 6,
  DEFAULT_PAGE_SIZE: 9,
  MAX_PAGE_SIZE: 100,
} as const;

// UI Configuration
export const BREAKPOINTS = {
  MOBILE: 768,
  TABLET: 1024,
  DESKTOP: 1200,
} as const;

// Default Values
export const DEFAULTS = {
  AVATAR: "/default-avatar.png",
  IMAGE: "/placeholder-image.jpg",
  AUTHOR: "Author Name", // TODO: Remove when user service is implemented
} as const;

// Error Messages
export const ERROR_MESSAGES = {
  GENERIC: "Something went wrong. Please try again.",
  NETWORK: "Network error. Please check your connection.",
  NOT_FOUND: "The requested content was not found.",
  UNAUTHORIZED: "You are not authorized to perform this action.",
  VALIDATION: "Please check your input and try again.",
  FAVORITE_ADD_SUCCESS: "Added to favorites!",
  FAVORITE_REMOVE_SUCCESS: "Removed from favorites!",
  FAVORITE_ERROR: "Failed to update favorite status",
} as const;

// Success Messages
export const SUCCESS_MESSAGES = {
  FAVORITE_ADDED: "Recipe saved!",
  FAVORITE_REMOVED: "Recipe removed from saved!",
  POST_CREATED: "Post created successfully!",
  POST_UPDATED: "Post updated successfully!",
  POST_DELETED: "Post deleted successfully!",
} as const;

// Component Variants
export const CARD_VARIANTS = {
  DEFAULT: "default",
  HORIZONTAL: "horizontal",
  FEATURED: "featured",
} as const;

// Sort Options
export const SORT_OPTIONS = {
  CREATED_DESC: { field: "createdAt", order: "desc" as const },
  CREATED_ASC: { field: "createdAt", order: "asc" as const },
  TITLE_ASC: { field: "title", order: "asc" as const },
  TITLE_DESC: { field: "title", order: "desc" as const },
  VIEWS_DESC: { field: "viewCount", order: "desc" as const },
} as const;

// Local Storage Keys
export const STORAGE_KEYS = {
  USER_PREFERENCES: "blog_user_preferences",
  THEME: "blog_theme",
  FAVORITES: "blog_favorites",
} as const;

// SEO Configuration
export const SEO = {
  DEFAULT_TITLE: "Pancakes Blog",
  DEFAULT_DESCRIPTION: "A modern blog platform built with React and TypeScript",
  DEFAULT_KEYWORDS: "blog, react, typescript, web development",
} as const;
