export interface BlogPost {
  id: string
  title: string
  content: string
  featuredImage?: string
  status: number // 0 = Draft, 1 = Published, 2 = Deleted
  authorId: string
  createdAt: string
  updatedAt: string
  publishedAt?: string
  authorName: string
  authorImage: string
}

export interface BlogPostSearchRequest {
  search?: string
  authorId?: string
  status?: number
  dateFrom?: string
  dateTo?: string
  page?: number
  pageSize?: number
  sortBy?: string
  sortOrder?: string
}

export interface BlogPostStats {
  totalPosts: number
  publishedPosts: number
  draftPosts: number
  deletedPosts: number
  totalComments: number
  totalViews: number
  averageRating: number
}

export enum BlogPostStatus {
  Draft = 0,
  Published = 1,
  Deleted = 2
}

export const BlogPostStatusLabels = {
  [BlogPostStatus.Draft]: 'Draft',
  [BlogPostStatus.Published]: 'Published', 
  [BlogPostStatus.Deleted]: 'Deleted'
}

export const BlogPostStatusColors = {
  [BlogPostStatus.Draft]: 'default',
  [BlogPostStatus.Published]: 'success',
  [BlogPostStatus.Deleted]: 'error'
} as const
