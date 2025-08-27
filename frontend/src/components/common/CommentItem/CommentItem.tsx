import React, { useState } from "react";
import { Typography, Button } from "antd";
import {
  UserOutlined,
  DeleteOutlined,
  EditOutlined,
  MessageOutlined,
} from "@ant-design/icons";
import type { CommentItemProps, CreateCommentDto } from "@/types/comment";
import CommentForm from "../CommentForm/CommentForm";
import CommentLikeButtons from "../CommentLikeButtons/CommentLikeButtons";
import { CachedAvatar, ReportButton } from "@/components/common";
import { getProfilePictureUrl } from "@/utils/imageUtils";
import { useAuth } from "@/contexts/AuthContext";
import { ReportContentType } from "@/types/report";
import "./CommentItem.scss";

const { Text, Paragraph } = Typography;

const CommentItem: React.FC<CommentItemProps> = ({
  comment,
  onEdit,
  onDelete,
  onReply,
  depth = 0,
}) => {
  const [showReplyForm, setShowReplyForm] = useState(false);
  const { user, isAuthenticated } = useAuth();

  // Helper function to check if current user can modify a comment
  const canModifyComment = (): boolean => {
    const result = Boolean(
      isAuthenticated &&
        user &&
        comment.authorId?.trim().toLowerCase() === user.id?.trim().toLowerCase()
    );

    return result;
  };
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffInMs = now.getTime() - date.getTime();
    const diffInDays = Math.floor(diffInMs / (1000 * 60 * 60 * 24));
    const diffInHours = Math.floor(diffInMs / (1000 * 60 * 60));
    const diffInMinutes = Math.floor(diffInMs / (1000 * 60));

    if (diffInDays > 0) {
      return `${diffInDays} day${diffInDays > 1 ? "s" : ""} ago`;
    } else if (diffInHours > 0) {
      return `${diffInHours} hour${diffInHours > 1 ? "s" : ""} ago`;
    } else if (diffInMinutes > 0) {
      return `${diffInMinutes} minute${diffInMinutes > 1 ? "s" : ""} ago`;
    } else {
      return "Just now";
    }
  };

  const handleEdit = () => {
    if (onEdit) {
      onEdit(comment);
    }
  };

  const handleDelete = () => {
    if (onDelete) {
      onDelete(comment.id);
    }
  };

  // Check if comment is soft deleted
  const isDeleted = comment.isDeleted || comment.content === "[deleted]";

  const handleReply = () => {
    setShowReplyForm(true);
  };

  const handleReplySubmit = async (replyData: CreateCommentDto) => {
    try {
      if (onReply) {
        await onReply(replyData);
      }
      setShowReplyForm(false);
    } catch (error) {
      // Error handling is done in the form
    }
  };

  const handleCancelReply = () => {
    setShowReplyForm(false);
  };

  return (
    <div className={`comment-item ${depth > 0 ? "comment-item--reply" : ""}`}>
      {/* Thread line for nested comments */}
      {depth > 0 && <div className="comment-item__thread-line" />}

      <div className="comment-item__main">
        <div className="comment-item__avatar">
          <CachedAvatar
            size={depth > 0 ? 32 : 40}
            src={comment.authorImage}
            fallbackSrc="/default-avatar.png"
            icon={<UserOutlined />}
            alt={comment.authorName}
          />
        </div>

        <div className="comment-item__content">
          <div className="comment-item__header">
            <div className="comment-item__meta">
              <Text
                strong
                className={`comment-item__author ${
                  isDeleted ? "comment-item__author--deleted" : ""
                }`}
              >
                {isDeleted ? "[deleted]" : comment.authorName}
              </Text>
              <Text className="comment-item__date">
                {formatDate(comment.createdAt)}
              </Text>
            </div>

            <div className="comment-item__actions">
              {!isDeleted && (
                <>
                  <CommentLikeButtons commentId={comment.id} size="small" />
                  {onReply &&
                    depth < 3 && ( // Allow up to 3 levels for better threading
                      <Button
                        type="text"
                        size="small"
                        icon={<MessageOutlined />}
                        onClick={handleReply}
                        className="comment-item__action-btn"
                      >
                        Reply
                      </Button>
                    )}
                  <ReportButton
                    contentType={ReportContentType.Comment}
                    contentId={comment.id}
                    contentTitle={
                      comment.content.length > 50
                        ? comment.content.substring(0, 50) + "..."
                        : comment.content
                    }
                    authorId={comment.authorId}
                    size="small"
                    type="text"
                    className="comment-item__action-btn"
                  />
                  {onEdit && canModifyComment() && (
                    <Button
                      type="text"
                      size="small"
                      icon={<EditOutlined />}
                      onClick={handleEdit}
                      className="comment-item__action-btn"
                    />
                  )}
                  {onDelete && canModifyComment() && (
                    <Button
                      type="text"
                      size="small"
                      icon={<DeleteOutlined />}
                      onClick={handleDelete}
                      className="comment-item__action-btn comment-item__action-btn--danger"
                    />
                  )}
                </>
              )}
            </div>
          </div>

          <Paragraph
            className={`comment-item__text ${
              isDeleted ? "comment-item__text--deleted" : ""
            }`}
          >
            {isDeleted ? "This message has been deleted" : comment.content}
          </Paragraph>

          {/* Reply form */}
          {showReplyForm && (
            <div className="comment-item__reply-form">
              <CommentForm
                blogPostId={comment.blogPostId}
                parentCommentId={comment.id}
                parentAuthor={comment.authorName}
                onSubmit={handleReplySubmit}
                onCancel={handleCancelReply}
                placeholder="Write a reply..."
              />
            </div>
          )}
        </div>
      </div>

      {/* Nested replies - positioned below the main comment */}
      {comment.replies && comment.replies.length > 0 && (
        <div className="comment-item__replies">
          {comment.replies.map((reply) => (
            <CommentItem
              key={reply.id}
              comment={reply}
              onEdit={onEdit}
              onDelete={onDelete}
              onReply={onReply}
              depth={depth + 1}
            />
          ))}
        </div>
      )}
    </div>
  );
};

export default CommentItem;
