import React from "react";
import { Button, Tooltip, App } from "antd";
import {
  LikeOutlined,
  DislikeOutlined,
  LikeFilled,
  DislikeFilled,
} from "@ant-design/icons";
import { useCommentLikes } from "@/hooks/useCommentLikes";
import type { CommentLikeButtonsProps } from "@/types/rating";
import "./CommentLikeButtons.scss";

const CommentLikeButtons: React.FC<CommentLikeButtonsProps> = ({
  commentId,
  size = "small",
}) => {
  const { stats, loading, toggleLike, toggleDislike } =
    useCommentLikes(commentId);
  const { message } = App.useApp();

  const handleLike = async () => {
    try {
      await toggleLike();
    } catch (error) {
      message.error("Failed to update like status");
    }
  };

  const handleDislike = async () => {
    try {
      await toggleDislike();
    } catch (error) {
      message.error("Failed to update dislike status");
    }
  };

  // Always render the buttons - they'll show 0 counts if no stats yet

  const likeCount = stats?.likeCount || 0;
  const dislikeCount = stats?.dislikeCount || 0;
  const userLike = stats?.userLike;

  // Map our custom sizes to Ant Design button sizes
  const buttonSize = size === "large" ? "middle" : "small";

  return (
    <div className={`comment-like-buttons comment-like-buttons--${size}`}>
      <Tooltip title={userLike === true ? "Remove like" : "Like this comment"}>
        <Button
          type="text"
          size={buttonSize}
          icon={userLike === true ? <LikeFilled /> : <LikeOutlined />}
          onClick={handleLike}
          loading={loading}
          className={`comment-like-buttons__like ${
            userLike === true ? "comment-like-buttons__like--active" : ""
          }`}
        >
          {likeCount > 0 && (
            <span className="comment-like-buttons__count">{likeCount}</span>
          )}
        </Button>
      </Tooltip>

      <Tooltip
        title={userLike === false ? "Remove dislike" : "Dislike this comment"}
      >
        <Button
          type="text"
          size={buttonSize}
          icon={userLike === false ? <DislikeFilled /> : <DislikeOutlined />}
          onClick={handleDislike}
          loading={loading}
          className={`comment-like-buttons__dislike ${
            userLike === false ? "comment-like-buttons__dislike--active" : ""
          }`}
        >
          {dislikeCount > 0 && (
            <span className="comment-like-buttons__count">{dislikeCount}</span>
          )}
        </Button>
      </Tooltip>
    </div>
  );
};

export default CommentLikeButtons;
