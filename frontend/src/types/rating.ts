// Post Rating interfaces
export interface PostRating {
  id: string;
  blogPostId: string;
  userId: string;
  rating: number;
  createdAt: string;
}

export interface CreatePostRatingDto {
  blogPostId: string;
  userId: string;
  rating: number; // 0.5 to 5.0 with 0.5 increments
}

export interface PostRatingStats {
  blogPostId: string;
  averageRating: number;
  totalRatings: number;
  userRating?: number;
  ratingDistribution: Record<number, number>;
}

// Comment Like interfaces
export interface CommentLike {
  id: string;
  commentId: string;
  userId: string;
  isLike: boolean;
  createdAt: string;
}

export interface CreateCommentLikeDto {
  commentId: string;
  userId: string;
  isLike: boolean; // true = like, false = dislike
}

export interface CommentLikeStats {
  commentId: string;
  likeCount: number;
  dislikeCount: number;
  userLike?: boolean; // null = no vote, true = liked, false = disliked
}

// UI Component Props
export interface GlazeMeterProps {
  blogPostId: string;
  averageRating: number;
  totalRatings: number;
  userRating?: number;
  onRate: (rating: number) => Promise<void>;
  readonly?: boolean;
}

export interface CommentLikeButtonsProps {
  commentId: string;
  size?: "small" | "medium" | "large";
}
