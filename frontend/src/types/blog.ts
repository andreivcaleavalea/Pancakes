export interface BlogPost {
  id: string;
  title: string;
  description: string;
  date: string;
  image: string;
  author?: string;
  authorAvatar?: string;
  isFeatured?: boolean;
  isPopular?: boolean;
}

export interface FeaturedPost extends BlogPost {
  isFeatured: boolean;
}
