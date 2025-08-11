import React from "react";
import { Typography, Spin, Alert, Empty, App } from "antd";
import { MessageOutlined } from "@ant-design/icons";
import { useComments } from "@/hooks/useComments";
import CommentForm from "../CommentForm/CommentForm";
import CommentItem from "../CommentItem/CommentItem";
import type { CommentSectionProps } from "@/types/comment";
import "./CommentSection.scss";

const { Title } = Typography;

const CommentSection: React.FC<CommentSectionProps> = ({ blogPostId }) => {
  const { comments, loading, error, addComment, updateComment, deleteComment } =
    useComments(blogPostId);
  const { modal, message } = App.useApp();

  const handleDeleteComment = (commentId: string) => {
    modal.confirm({
      title: "Delete Comment",
      content:
        "Are you sure you want to delete this comment? This action cannot be undone.",
      okText: "Delete",
      okType: "danger",
      cancelText: "Cancel",
      onOk: async () => {
        try {
          await deleteComment(commentId);
          message.success("Comment deleted successfully");
        } catch (error) {
          message.error("Failed to delete comment");
        }
      },
    });
  };

  const handleEditComment = async (comment: Comment) => {
    // Create a simple input modal for editing
    let newContent = comment.content;

    modal.confirm({
      title: "Edit Comment",
      content: (
        <div style={{ marginTop: 16 }}>
          <textarea
            defaultValue={comment.content}
            onChange={(e) => (newContent = e.target.value)}
            style={{
              width: "100%",
              minHeight: "100px",
              padding: "8px",
              border: "1px solid #d9d9d9",
              borderRadius: "4px",
              resize: "vertical",
            }}
            placeholder="Edit your comment..."
          />
        </div>
      ),
      okText: "Save",
      cancelText: "Cancel",
      onOk: async () => {
        if (newContent.trim() && newContent !== comment.content) {
          try {
            await updateComment(comment.id, {
              content: newContent.trim(),
              blogPostId: comment.blogPostId,
              parentCommentId: comment.parentCommentId,
            });
            message.success("Comment updated successfully");
          } catch (error) {
            message.error("Failed to update comment");
          }
        }
      },
    });
  };

  // const handleReply = async (parentComment: any) => {
  //   // This will be handled by the CommentItem component's reply form
  //   // The actual reply submission goes through addComment with parentCommentId
  // };

  return (
    <div className="comment-section">
      <div className="comment-section__header">
        <Title level={3} className="comment-section__title">
          <MessageOutlined /> Comments ({comments.length})
        </Title>
      </div>

      <CommentForm
        blogPostId={blogPostId}
        onSubmit={addComment}
        loading={loading}
      />

      <div className="comment-section__list">
        {loading ? (
          <div className="comment-section__loading">
            <Spin size="large" />
          </div>
        ) : error ? (
          <Alert
            message="Error Loading Comments"
            description={error}
            type="error"
            showIcon
          />
        ) : comments.length === 0 ? (
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description="No comments yet"
            className="comment-section__empty"
          >
            <p>Be the first to share your thoughts about this recipe!</p>
          </Empty>
        ) : (
          <div className="comment-section__items">
            {comments.map((comment) => (
              <CommentItem
                key={comment.id}
                comment={comment}
                onEdit={handleEditComment}
                onDelete={handleDeleteComment}
                onReply={addComment}
                depth={0}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default CommentSection;
