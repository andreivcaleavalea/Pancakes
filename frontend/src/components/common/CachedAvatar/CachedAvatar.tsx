import React, { useState, useEffect, useRef } from "react";
import { Avatar } from "antd";
import type { AvatarProps } from "antd";

// Simple in-memory cache for images
const imageCache = new Map<string, string>();
const loadingImages = new Set<string>();

interface CachedAvatarProps extends Omit<AvatarProps, "src"> {
  src?: string;
  fallbackSrc?: string;
}

const CachedAvatar: React.FC<CachedAvatarProps> = ({
  src,
  fallbackSrc = "/default-avatar.png",
  children,
  ...props
}) => {
  const [imageSrc, setImageSrc] = useState<string | undefined>(src);
  const [hasError, setHasError] = useState(false);
  const mountedRef = useRef(true);

  useEffect(() => {
    mountedRef.current = true;
    return () => {
      mountedRef.current = false;
    };
  }, []);

  useEffect(() => {
    if (!src) {
      setImageSrc(undefined);
      setHasError(false);
      return;
    }

    // Handle external URLs (like OAuth profile pictures) - use directly
    if (src.startsWith('http')) {
      setImageSrc(src);
      setHasError(false);
      return;
    }

    // For local files, construct the full URL
    const fullSrc = `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${src}`;

    // Check if image is already cached
    if (imageCache.has(fullSrc)) {
      setImageSrc(imageCache.get(fullSrc));
      setHasError(false);
      return;
    }

    // Check if image is currently being loaded
    if (loadingImages.has(fullSrc)) {
      // Wait for the existing load to complete
      const checkCache = setInterval(() => {
        if (imageCache.has(fullSrc)) {
          if (mountedRef.current) {
            setImageSrc(imageCache.get(fullSrc));
            setHasError(false);
          }
          clearInterval(checkCache);
        }
      }, 100);

      // Clean up interval after 10 seconds
      setTimeout(() => clearInterval(checkCache), 10000);
      return;
    }

    // Load the image
    loadingImages.add(fullSrc);
    const img = new Image();

    img.onload = () => {
      loadingImages.delete(fullSrc);
      imageCache.set(fullSrc, fullSrc);
      if (mountedRef.current) {
        setImageSrc(fullSrc);
        setHasError(false);
      }
    };

    img.onerror = () => {
      loadingImages.delete(fullSrc);
      imageCache.set(fullSrc, fallbackSrc);
      if (mountedRef.current) {
        setImageSrc(fallbackSrc);
        setHasError(true);
      }
    };

    // Add a timeout to prevent hanging requests
    setTimeout(() => {
      if (loadingImages.has(fullSrc)) {
        loadingImages.delete(fullSrc);
        imageCache.set(fullSrc, fallbackSrc);
        if (mountedRef.current) {
          setImageSrc(fallbackSrc);
          setHasError(true);
        }
      }
    }, 5000); // 5 second timeout

    img.src = fullSrc;
  }, [src, fallbackSrc]);

  const handleError = () => {
    if (imageSrc !== fallbackSrc) {
      setImageSrc(fallbackSrc);
      setHasError(true);
      if (src) {
        imageCache.set(src, fallbackSrc);
      }
    }
    return false;
  };

  return (
    <Avatar
      {...props}
      src={hasError ? undefined : imageSrc}
      onError={handleError}
    >
      {children}
    </Avatar>
  );
};

export default CachedAvatar;
