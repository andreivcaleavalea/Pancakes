export interface BlogPost {
  id: string;
  title: string;
  description: string;
  date: string;
  image: string;
  author?: string;
}

export interface FeaturedPost extends BlogPost {
  isFeatured: boolean;
}
