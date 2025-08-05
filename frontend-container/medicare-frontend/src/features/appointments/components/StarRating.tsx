import React from "react";
import { Star } from "lucide-react";

interface StarRatingProps {
  rating: number;
  onRatingChange?: (rating: number) => void;
  maxRating?: number;
  size?: number;
  readOnly?: boolean;
  className?: string;
}

export const StarRating: React.FC<StarRatingProps> = ({
  rating,
  onRatingChange,
  maxRating = 5,
  size = 20,
  readOnly = false,
  className = "",
}) => {
  const handleStarClick = (starIndex: number) => {
    if (!readOnly && onRatingChange) {
      onRatingChange(starIndex + 1);
    }
  };

  return (
    <div className={`flex items-center gap-1 ${className}`}>
      {Array.from({ length: maxRating }, (_, index) => {
        const isFilled = index < rating;
        return (
          <button
            key={index}
            type="button"
            onClick={() => handleStarClick(index)}
            disabled={readOnly}
            className={`transition-colors ${
              readOnly ? "cursor-default" : "cursor-pointer hover:scale-110"
            }`}
          >
            <Star
              size={size}
              className={
                isFilled
                  ? "fill-yellow-400 text-yellow-400"
                  : "text-gray-300 hover:text-yellow-300"
              }
            />
          </button>
        );
      })}
    </div>
  );
};
