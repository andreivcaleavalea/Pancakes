import React, { useState } from "react";
import { Rate, Typography } from "antd";
import { StarFilled } from "@ant-design/icons";
import type { GlazeMeterProps } from "@/types/rating";
import "./GlazeMeter.scss";

const { Text } = Typography;

const GlazeMeter: React.FC<GlazeMeterProps> = ({
  // blogPostId,
  averageRating,
  totalRatings,
  userRating,
  onRate,
  readonly = false,
}) => {
  const [hoverRating, setHoverRating] = useState<number | undefined>();

  const handleRateChange = async (value: number) => {
    if (!readonly && onRate) {
      if (value > 0) {
        try {
          await onRate(value);
        } catch (error) {
          console.error("Failed to submit rating:", error);
        }
      }
    }
  };

  const displayRating = hoverRating || userRating || 0;
  const syrupPercentage = (averageRating / 5) * 100;

  const getRatingText = (rating: number) => {
    if (rating === 0) return "Not rated";
    if (rating <= 1.5) return "Needs more syrup 🥞";
    if (rating <= 2.5) return "Light glaze 🍯";
    if (rating <= 3.5) return "Sweet glaze ✨";
    if (rating <= 4.5) return "Perfect glaze! 🔥";
    return "Pancake perfection! 🏆";
  };

  return (
    <div className="glaze-meter">
      <div className="glaze-meter__header">
        <div className="glaze-meter__title">
          <span className="glaze-meter__icon">🥞</span>
          <Text strong>Glaze Meter</Text>
        </div>
        <div className="glaze-meter__stats">
          <Text className="glaze-meter__rating">
            {averageRating > 0 ? averageRating.toFixed(1) : "0.0"}
          </Text>
          <Text className="glaze-meter__count">
            ({totalRatings} {totalRatings === 1 ? "rating" : "ratings"})
          </Text>
        </div>
      </div>

      {/* Visual Syrup Bar */}
      <div className="glaze-meter__visual">
        <div className="glaze-meter__pancake">
          <div className="glaze-meter__syrup-container">
            <div
              className="glaze-meter__syrup-fill"
              style={{ width: `${syrupPercentage}%` }}
            />
            <div className="glaze-meter__syrup-glow" />
          </div>
          <div className="glaze-meter__rating-text">
            {getRatingText(averageRating)}
          </div>
        </div>
      </div>

      {/* Interactive Rating */}
      {!readonly && (
        <div className="glaze-meter__interactive">
          <Text className="glaze-meter__prompt">Rate this recipe:</Text>
          <Rate
            allowHalf
            allowClear={false}
            value={displayRating}
            onChange={handleRateChange}
            onHoverChange={setHoverRating}
            character={<StarFilled />}
            className="glaze-meter__stars"
            tooltips={["Poor", "Fair", "Good", "Very Good", "Excellent"]}
          />
          {displayRating > 0 && (
            <Text className="glaze-meter__user-rating">
              Your rating: {displayRating}/5
            </Text>
          )}
        </div>
      )}

      {/* User's Current Rating Display */}
      {readonly && userRating && (
        <div className="glaze-meter__user-display">
          <Text>Your rating: </Text>
          <Rate
            disabled
            allowHalf
            value={userRating}
            character={<StarFilled />}
            className="glaze-meter__user-stars"
          />
          <Text>({userRating}/5)</Text>
        </div>
      )}
    </div>
  );
};

export default GlazeMeter;
