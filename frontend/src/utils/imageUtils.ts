import { blogImageApi } from "@/services/blogImageApi";

/**
 * Get the proper image URL for display - handles both uploaded and external images
 * @param imagePath - The image path from the blog post
 * @returns The full URL to display the image
 */
export const getImageUrl = (imagePath: string | undefined | null): string => {
  if (!imagePath) {
    return "/placeholder-image.jpg"; // Default placeholder
  }

  // If it's already a full URL (external image), return as-is
  if (imagePath.startsWith("http://") || imagePath.startsWith("https://")) {
    return imagePath;
  }

  // If it's a relative path to uploaded image, use the blog image API
  return blogImageApi.getImageUrl(imagePath);
};

/**
 * Get the proper profile picture URL - for backward compatibility with UserService
 * @param imagePath - The image path from user profile
 * @returns The full URL to display the profile picture
 */
export const getProfilePictureUrl = (
  imagePath: string | undefined | null
): string => {
  if (!imagePath) {
    return "/default-avatar.png"; // Default avatar
  }

  // If it's already a full URL (external image), return as-is
  if (imagePath.startsWith("http://") || imagePath.startsWith("https://")) {
    return imagePath;
  }

  // For uploaded assets, construct full URL with API base
  if (imagePath.startsWith("assets/")) {
    const USER_API_BASE = import.meta.env.VITE_USER_API_URL;
    if (!USER_API_BASE) {
      throw new Error("VITE_USER_API_URL environment variable is required for uploaded assets");
    }
    return `${USER_API_BASE}/${imagePath}`;
  }

  // For frontend-served assets (like default avatar), return as relative path
  return imagePath.startsWith("/") ? imagePath : `/${imagePath}`;
};

/**
 * Check if an image path is an uploaded image (vs external URL)
 * @param imagePath - The image path to check
 * @returns true if it's an uploaded image, false if external
 */
export const isUploadedImage = (imagePath: string): boolean => {
  return (
    imagePath.includes("assets/blog-images/") ||
    imagePath.includes("assets/profile-pictures/")
  );
};

/**
 * Extract filename from image URL for deletion purposes
 * @param imageUrl - Full image URL
 * @returns filename if it's an uploaded image, null otherwise
 */
export const extractFilenameFromUrl = (imageUrl: string): string | null => {
  if (!isUploadedImage(imageUrl)) {
    return null;
  }

  const parts = imageUrl.split("/");
  return parts[parts.length - 1] || null;
};

/**
 * Validation result interface for profile picture uploads
 */
export interface ProfilePictureValidation {
  isValid: boolean;
  error?: string;
}

/**
 * Validate profile picture file before upload
 * @param file - The file to validate
 * @returns Validation result with isValid flag and optional error message
 */
export const validateProfilePicture = (
  file: File
): ProfilePictureValidation => {
  // Check if it's an image file
  const isImage = file.type.startsWith("image/");
  if (!isImage) {
    return {
      isValid: false,
      error: "You can only upload image files!",
    };
  }

  // Check file size (5MB limit for profile pictures - smaller than blog images)
  const maxSize = 5 * 1024 * 1024; // 5MB
  if (file.size > maxSize) {
    return {
      isValid: false,
      error: "Profile picture must be smaller than 5MB!",
    };
  }

  // Check for supported image formats
  const supportedFormats = [
    "image/jpeg",
    "image/jpg",
    "image/png",
    "image/gif",
    "image/webp",
  ];
  if (!supportedFormats.includes(file.type)) {
    return {
      isValid: false,
      error: "Only JPEG, PNG, GIF, and WebP formats are supported!",
    };
  }

  return {
    isValid: true,
  };
};
