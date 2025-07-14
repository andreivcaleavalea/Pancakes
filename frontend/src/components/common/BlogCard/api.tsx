import { blogData } from '@utils/mockData';
import { type BlogPost, type FeaturedPost } from '@/types/blog';

export const fetchBlogPost = async (id: string): Promise<BlogPost | FeaturedPost | null> => {
  // TODO: Replace with actual API call when backend is ready
  // Example:
  // const response = await fetch(`/api/posts/${id}`);
  // const data = await response.json();
  // return data;
  
  // Using mock data for now
  const allPosts = [
    ...blogData.featuredPosts,
    ...blogData.horizontalPosts,
    ...blogData.gridPosts
  ];
  
  return allPosts.find(post => post.id === id) || null;
};

export const toggleFavorite = async (id: string, isFavorite: boolean): Promise<boolean> => {
  // TODO: Replace with actual API call when backend is ready
  // Example:
  // const response = await fetch(`/api/posts/${id}/favorite`, {
  //   method: 'POST',
  //   body: JSON.stringify({ isFavorite }),
  //   headers: { 'Content-Type': 'application/json' }
  // });
  // const data = await response.json();
  // return data.success;
  
  // Mock response
  console.log(`Post ${id} favorite status set to ${isFavorite}`);
  return true;
}; 