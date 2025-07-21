// Enums to match backend
export const PostStatus = {
  Draft: 0,
  Published: 1,
  Archived: 2,
} as const;

export type PostStatus = (typeof PostStatus)[keyof typeof PostStatus];

// Core interfaces matching BlogService DTOs
export interface BlogPost {
  id: string;
  title: string;
  slug: string;
  content: string;
  excerpt?: string;
  featuredImage?: string;
  status: PostStatus;
  authorId: string;
  viewCount: number;
  isFeatured: boolean;
  isPopular: boolean;
  createdAt: string;
  updatedAt: string;
  publishedAt?: string;
}

export interface FeaturedPost extends BlogPost {
  isFeatured: true;
}

// API Response types
export interface PaginationInfo {
  currentPage: number;
  pageSize: number;
  totalPages: number;
  totalItems: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface PaginatedResult<T> {
  data: T[];
  pagination: PaginationInfo;
}

// Request types for API calls
export interface BlogPostQueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  authorId?: string;
  status?: PostStatus;
  isFeatured?: boolean;
  isPopular?: boolean;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
  dateFrom?: string;
  dateTo?: string;
}

export interface CreateBlogPostDto {
  title: string;
  content: string;
  featuredImage?: string;
  status: PostStatus;
  authorId: string;
  publishedAt?: string;
}

// UI Props types
export interface BlogCardProps {
  post: BlogPost;
  variant?: "default" | "horizontal" | "featured";
  onClick?: (post: BlogPost) => void;
}

// Hook return types
export interface UseBlogDataResult {
  data: {
    featuredPosts: FeaturedPost[];
    horizontalPosts: BlogPost[];
    gridPosts: BlogPost[];
  };
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export interface UsePaginatedPostsResult {
  data: BlogPost[];
  pagination: PaginationInfo;
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}
