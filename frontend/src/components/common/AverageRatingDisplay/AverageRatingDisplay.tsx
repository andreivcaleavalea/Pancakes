import React from "react";
import { Rate, Typography } from "antd";
import { StarFilled } from "@ant-design/icons";
import "./AverageRatingDisplay.scss";

const { Text } = Typography;

interface AverageRatingDisplayProps {
  averageRating: number;
  totalRatings: number;
  size?: "small" | "default";
}

const AverageRatingDisplay: React.FC<AverageRatingDisplayProps> = ({
  averageRating,
  totalRatings,
  size = "default",
}) => {
  return (
    <div className={`average-rating-display average-rating-display--${size}`}>
      <div className="average-rating-display__stars">
        <Rate
          disabled
          allowHalf
          value={averageRating}
          character={<StarFilled />}
          className="average-rating-display__rate"
        />
      </div>
      <div className="average-rating-display__info">
        <Text className="average-rating-display__value">
          {averageRating.toFixed(1)}
        </Text>
        <Text className="average-rating-display__count">
          ({totalRatings} {totalRatings === 1 ? "rating" : "ratings"})
        </Text>
      </div>
    </div>
  );
};

export default AverageRatingDisplay;
