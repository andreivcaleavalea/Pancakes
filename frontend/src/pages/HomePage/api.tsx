import { blogData } from '@utils/mockData';
import type { BlogPost, FeaturedPost } from '@/types/blog';

interface BlogDataResponse {
  featuredPosts: FeaturedPost[];
  horizontalPosts: BlogPost[];
  gridPosts: BlogPost[];
}

export const fetchBlogData = async (): Promise<BlogDataResponse> => {
  // TODO: Replace with actual API call when backend is ready
  // Example:
  // const response = await fetch('/api/blog-data');
  // const data = await response.json();
  // return data;
  
  // Using mock data for now
  return {
    featuredPosts: blogData.featuredPosts,
    horizontalPosts: blogData.horizontalPosts,
    gridPosts: blogData.gridPosts,
  };
};

export const fetchPaginatedPosts = async (page: number, postsPerPage: number) => {
  // TODO: Replace with actual API call when backend is ready
  // Example:
  // const response = await fetch(`/api/posts?page=${page}&limit=${postsPerPage}`);
  // const data = await response.json();
  // return data;
  
  // Using mock data and client-side pagination for now
  const indexOfLastPost = page * postsPerPage;
  const indexOfFirstPost = indexOfLastPost - postsPerPage;
  return {
    posts: blogData.gridPosts.slice(indexOfFirstPost, indexOfLastPost),
    total: blogData.gridPosts.length,
    totalPages: Math.ceil(blogData.gridPosts.length / postsPerPage)
  };
}; 