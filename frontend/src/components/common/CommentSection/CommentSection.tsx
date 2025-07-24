import React, { useState } from "react";
import { Typography, Divider, Button, Space } from "antd";
import { LoginOutlined } from "@ant-design/icons";
import CommentForm from "../CommentForm/CommentForm";
import CommentItem from "../CommentItem/CommentItem";
import { useComments } from "@/hooks/useComments";
import { useAuth } from "@/contexts/AuthContext";
import { useRouter } from "@/router/RouterProvider";
import type { Comment, CreateCommentDto } from "@/types/comment";
import "./CommentSection.scss";

const { Title, Text } = Typography;

interface CommentSectionProps {
  blogPostId: string;
}

const CommentSection: React.FC<CommentSectionProps> = ({ blogPostId }) => {
  const { comments, loading, error, addComment, deleteComment } =
    useComments(blogPostId);
  const { isAuthenticated } = useAuth();
  const { navigate } = useRouter();
  const [submittingComment, setSubmittingComment] = useState(false);

  const handleAddComment = async (commentData: CreateCommentDto) => {
    if (!isAuthenticated) {
      navigate("login");
      return;
    }

    setSubmittingComment(true);
    try {
      await addComment(commentData);
    } finally {
      setSubmittingComment(false);
    }
  };

  const handleReply = async (replyData: CreateCommentDto) => {
    if (!isAuthenticated) {
      navigate("login");
      return;
    }

    await addComment(replyData);
  };

  const handleEdit = (comment: Comment) => {
    console.log("Edit comment:", comment);
    // TODO: Implement edit functionality
  };

  const handleDelete = async (commentId: string) => {
    if (!isAuthenticated) {
      navigate("login");
      return;
    }

    await deleteComment(commentId);
  };

  const handleLoginClick = () => {
    navigate("login");
  };

  if (loading) {
    return <div className="comment-section__loading">Loading comments...</div>;
  }

  if (error) {
    return (
      <div className="comment-section__error">
        Error loading comments: {error}
      </div>
    );
  }

  return (
    <div className="comment-section">
      <Title level={3} className="comment-section__title">
        Comments ({comments.length})
      </Title>

      <Divider />

      {/* Comment Form - shown only to authenticated users */}
      {isAuthenticated ? (
        <div className="comment-section__form">
          <CommentForm
            blogPostId={blogPostId}
            onSubmit={handleAddComment}
            loading={submittingComment}
            placeholder="Share your thoughts about this recipe..."
          />
        </div>
      ) : (
        <div className="comment-section__login-prompt">
          <Text className="comment-section__login-text">
            Please log in to leave a comment
          </Text>
          <Button
            type="primary"
            icon={<LoginOutlined />}
            onClick={handleLoginClick}
            className="comment-section__login-button"
          >
            Log In
          </Button>
        </div>
      )}

      <Divider />

      {/* Comments List */}
      <div className="comment-section__list">
        {comments.length === 0 ? (
          <div className="comment-section__empty">
            <Text type="secondary">
              {isAuthenticated
                ? "Be the first to comment!"
                : "No comments yet. Log in to be the first to comment!"}
            </Text>
          </div>
        ) : (
          <Space direction="vertical" size="large" style={{ width: "100%" }}>
            {comments.map((comment) => (
              <CommentItem
                key={comment.id}
                comment={comment}
                onEdit={handleEdit}
                onDelete={handleDelete}
                onReply={handleReply}
                depth={0}
              />
            ))}
          </Space>
        )}
      </div>
    </div>
  );
};

export default CommentSection;
