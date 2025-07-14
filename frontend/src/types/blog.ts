export interface BlogPost {
  id: string;
  title: string;
  description: string;
  date: string;
  image: string;
  tags: Tag[];
  author?: string;
}

export interface Tag {
  name: string;
  color: string;
  backgroundColor: string;
}

export interface FeaturedPost extends BlogPost {
  isFeatured: boolean;
}
