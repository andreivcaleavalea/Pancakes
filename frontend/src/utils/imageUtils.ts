export function getProfilePictureUrl(imagePath?: string): string | undefined {
  if (!imagePath) return undefined;
  
  const baseUrl = import.meta.env.VITE_USER_API_URL || 'http://localhost:5141';
  return `${baseUrl}/${imagePath}`;
}

export function validateProfilePicture(file: File): { isValid: boolean; error?: string } {
  const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
  const maxSize = 5 * 1024 * 1024; // 5MB

  if (!allowedTypes.includes(file.type)) {
    return { 
      isValid: false, 
      error: 'Only image files (JPEG, PNG, GIF, WebP) are allowed!' 
    };
  }

  if (file.size > maxSize) {
    return { 
      isValid: false, 
      error: 'Image must be smaller than 5MB!' 
    };
  }

  return { isValid: true };
} 