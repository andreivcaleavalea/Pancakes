import React, { useState, useRef, useEffect } from "react";
import { Skeleton } from "antd";

interface LazyImageProps {
  src: string | undefined;
  alt: string;
  className?: string;
  style?: React.CSSProperties;
  placeholder?: string;
  skeletonHeight?: number;
  onLoad?: () => void;
  onError?: () => void;
}

const LazyImage: React.FC<LazyImageProps> = ({
  src,
  alt,
  className = "",
  style,
  placeholder = "/placeholder-image.jpg",
  skeletonHeight = 200,
  onLoad,
  onError,
}) => {
  const [isLoaded, setIsLoaded] = useState(false);
  const [isInView, setIsInView] = useState(false);
  const [hasError, setHasError] = useState(false);
  const imgRef = useRef<HTMLImageElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsInView(true);
          observer.disconnect();
        }
      },
      {
        rootMargin: "50px", // Start loading 50px before the image enters viewport
        threshold: 0.1,
      }
    );

    observer.observe(container);

    return () => observer.disconnect();
  }, []);

  const handleLoad = () => {
    setIsLoaded(true);
    onLoad?.();
  };

  const handleError = () => {
    setHasError(true);
    onError?.();
  };

  const imageSrc = src || placeholder;

  // Check if this is a horizontal card (different layout requirements)
  const isHorizontal = className?.includes("horizontal");

  const containerStyle = isHorizontal
    ? {
        position: "relative",
        width: "180px", // Match CSS width for horizontal images
        height: "120px", // Match CSS height for horizontal images
        overflow: "hidden", // Constrain skeleton animation to container
        flexShrink: 0, // Prevent shrinking in flex container
        borderRadius: "8px",
        ...style,
      }
    : {
        position: "relative",
        width: "100%",
        height: skeletonHeight,
        overflow: "hidden", // Constrain skeleton animation to container
        ...style,
      };

  const skeletonStyle = isHorizontal
    ? {
        width: "100%",
        height: "100%",
        maxWidth: "100%",
        maxHeight: "100%",
      }
    : {
        width: "100%",
        height: skeletonHeight,
        maxWidth: "100%",
        maxHeight: "100%",
      };

  const imageStyle = isHorizontal
    ? {
        opacity: isLoaded ? 1 : 0,
        transition: "opacity 0.3s ease-in-out",
        width: "100%",
        height: "100%",
        objectFit: "cover",
        borderRadius: "8px",
      }
    : {
        opacity: isLoaded ? 1 : 0,
        transition: "opacity 0.3s ease-in-out",
        width: "100%",
        height: "100%",
        objectFit: "cover",
        position: "absolute",
        top: 0,
        left: 0,
        zIndex: 2,
      };

  return (
    <div
      ref={containerRef}
      className={`lazy-image-container ${className}`}
      style={containerStyle}
    >
      {!isInView ? (
        <Skeleton.Image style={skeletonStyle} active />
      ) : (
        <>
          {!isLoaded && !hasError && (
            <Skeleton.Image
              style={{
                position: "absolute",
                top: 0,
                left: 0,
                width: "100%",
                height: "100%",
                maxWidth: "100%",
                maxHeight: "100%",
                zIndex: 1,
              }}
              active
            />
          )}
          <img
            ref={imgRef}
            src={hasError ? placeholder : imageSrc}
            alt={alt}
            style={imageStyle}
            onLoad={handleLoad}
            onError={handleError}
            loading="lazy" // Native lazy loading as fallback
          />
        </>
      )}
    </div>
  );
};

export default LazyImage;
