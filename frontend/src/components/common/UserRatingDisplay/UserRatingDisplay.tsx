import React from "react";
import { Rate } from "antd";
import { StarFilled } from "@ant-design/icons";
import "./UserRatingDisplay.scss";

interface UserRatingDisplayProps {
  rating: number;
  size?: "small" | "default";
  showValue?: boolean;
}

const UserRatingDisplay: React.FC<UserRatingDisplayProps> = ({
  rating,
  size = "small",
  showValue = true,
}) => {
  return (
    <div className={`user-rating-display user-rating-display--${size}`}>
      <Rate
        disabled
        allowHalf
        value={rating}
        character={<StarFilled />}
        className="user-rating-display__stars"
      />
      {showValue && (
        <span className="user-rating-display__value">{rating}</span>
      )}
    </div>
  );
};

export default UserRatingDisplay;
