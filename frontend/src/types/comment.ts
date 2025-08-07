// Comment interfaces matching backend DTOs
export interface Comment {
  id: string;
  content: string;
  authorName: string;
  authorId: string;
  authorImage: string;
  blogPostId: string;
  parentCommentId?: string;
  replies: Comment[];
  createdAt: string;
  updatedAt: string;
  isDeleted?: boolean;
  deletedAt?: string;
}

export interface CreateCommentDto {
  content: string;
  authorName?: string; // Optional since backend will populate this
  authorId?: string; // Optional since backend will populate this
  blogPostId: string;
  parentCommentId?: string;
}

// UI Props types
export interface CommentItemProps {
  comment: Comment;
  onEdit?: (comment: Comment) => void;
  onDelete?: (commentId: string) => void;
  onReply?: (replyData: CreateCommentDto) => Promise<void>;
  depth?: number; // For styling nested comments
}

export interface CommentFormProps {
  blogPostId: string;
  parentCommentId?: string;
  parentAuthor?: string; // To show "Replying to @username"
  onSubmit: (comment: CreateCommentDto) => Promise<void>;
  onCancel?: () => void;
  loading?: boolean;
  placeholder?: string;
}

export interface CommentSectionProps {
  blogPostId: string;
}

// Hook return types
export interface UseCommentsResult {
  comments: Comment[];
  loading: boolean;
  error: string | null;
  addComment: (commentData: CreateCommentDto) => Promise<void>;
  updateComment: (id: string, commentData: CreateCommentDto) => Promise<void>;
  deleteComment: (id: string) => Promise<void>;
  refetch: () => Promise<void>;
}
