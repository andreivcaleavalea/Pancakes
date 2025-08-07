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
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleRateChange = async (value: number) => {
    if (!readonly && onRate && !isSubmitting) {
      if (value > 0) {
        try {
          setIsSubmitting(true);
          console.log(
            `⭐ Submitting rating: ${value} (current userRating: ${userRating})`
          );
          await onRate(value);
          console.log(`✅ Rating submitted successfully: ${value}`);
        } catch (error) {
          console.error("Failed to submit rating:", error);
        } finally {
          setIsSubmitting(false);
        }
      }
    }
  };

  // Use userRating when not hovering, fallback to 0 if no rating
  const displayRating =
    hoverRating !== undefined ? hoverRating : userRating || 0;

  // Log rating changes for debugging
  React.useEffect(() => {
    console.log(
      `🎯 GlazeMeter state - userRating: ${userRating}, displayRating: ${displayRating}, hoverRating: ${hoverRating}`
    );
  }, [userRating, displayRating, hoverRating]);
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
          <Text className="glaze-meter__prompt">
            {isSubmitting ? "Submitting..." : "Rate this recipe:"}
          </Text>
          <Rate
            key={`rating-${userRating || "none"}`}
            allowHalf
            allowClear={false}
            value={displayRating}
            onChange={handleRateChange}
            onHoverChange={setHoverRating}
            character={<StarFilled />}
            className="glaze-meter__stars"
            tooltips={["Poor", "Fair", "Good", "Very Good", "Excellent"]}
            disabled={isSubmitting}
          />
          {displayRating > 0 && (
            <Text className="glaze-meter__user-rating">
              {userRating
                ? `Your rating: ${userRating}/5`
                : `Preview: ${displayRating}/5`}
            </Text>
          )}
        </div>
      )}

      {/* User's Current Rating Display */}
      {readonly && userRating && (
        <div className="glaze-meter__user-display">
          <Text>Your rating: </Text>
          <Rate
            key={`readonly-rating-${userRating}`}
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
