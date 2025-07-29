import { useState, useEffect, useCallback } from "react";
import { commentsApi } from "@/services/commentApi";
import type {
  Comment,
  CreateCommentDto,
  UseCommentsResult,
} from "@/types/comment";

/**
 * Custom hook for managing comments for a specific blog post
 */
export const useComments = (blogPostId: string): UseCommentsResult => {
  const [comments, setComments] = useState<Comment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchComments = useCallback(async () => {
    if (!blogPostId) {
      setError("No blog post ID provided");
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const commentsData = await commentsApi.getByBlogPostId(blogPostId);
      setComments(commentsData);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load comments");
    } finally {
      setLoading(false);
    }
  }, [blogPostId]);

  const addComment = useCallback(async (commentData: CreateCommentDto) => {
    try {
      const newComment = await commentsApi.create(commentData);

      if (commentData.parentCommentId) {
        // This is a reply - need to add it to the correct parent's replies array
        setComments((prev) => {
          const addReplyToComment = (comments: Comment[]): Comment[] => {
            return comments.map((comment) => {
              if (comment.id === commentData.parentCommentId) {
                // Found the parent - add reply to its replies array
                return {
                  ...comment,
                  replies: [...comment.replies, newComment],
                };
              } else if (comment.replies.length > 0) {
                // Recursively search in nested replies
                return {
                  ...comment,
                  replies: addReplyToComment(comment.replies),
                };
              }
              return comment;
            });
          };

          return addReplyToComment(prev);
        });
      } else {
        // This is a top-level comment
        setComments((prev) => [newComment, ...prev]);
      }
    } catch (err) {
      throw new Error(
        err instanceof Error ? err.message : "Failed to add comment"
      );
    }
  }, []);

  const updateComment = useCallback(
    async (id: string, commentData: CreateCommentDto) => {
      try {
        const updatedComment = await commentsApi.update(id, commentData);
        setComments((prev) => {
          const updateCommentRecursively = (comments: Comment[]): Comment[] => {
            return comments.map((comment) => {
              if (comment.id === id) {
                return updatedComment;
              } else if (comment.replies.length > 0) {
                return {
                  ...comment,
                  replies: updateCommentRecursively(comment.replies),
                };
              }
              return comment;
            });
          };

          return updateCommentRecursively(prev);
        });
      } catch (err) {
        throw new Error(
          err instanceof Error ? err.message : "Failed to update comment"
        );
      }
    },
    []
  );

  const deleteComment = useCallback(async (id: string) => {
    try {
      await commentsApi.delete(id);
      setComments((prev) => {
        const removeCommentRecursively = (comments: Comment[]): Comment[] => {
          return comments
            .filter((comment) => comment.id !== id) // Remove if this is the comment to delete
            .map((comment) => ({
              ...comment,
              replies: removeCommentRecursively(comment.replies), // Recursively remove from replies
            }));
        };

        return removeCommentRecursively(prev);
      });
    } catch (err) {
      throw new Error(
        err instanceof Error ? err.message : "Failed to delete comment"
      );
    }
  }, []);

  useEffect(() => {
    fetchComments();
  }, [fetchComments]);

  return {
    comments,
    loading,
    error,
    addComment,
    updateComment,
    deleteComment,
    refetch: fetchComments,
  };
};
