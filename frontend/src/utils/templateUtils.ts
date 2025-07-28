// Shared utility functions for advanced settings across all templates

export const getBackgroundWithPattern = (color: string, pattern: string, opacity: number): string => {
  // If no color is provided, use transparent as default
  const backgroundColor = color && typeof color === 'string' && color.trim() !== '' ? color : 'transparent';
  
  if (pattern === 'none') return backgroundColor;
  
  const alpha = opacity * 0.15; // Slightly more visible patterns
  
  switch (pattern) {
    case 'dots':
      return `
        radial-gradient(circle at 2px 2px, rgba(0,0,0,${alpha}) 1px, transparent 1px),
        ${backgroundColor}
      `;
    case 'grid':
      return `
        linear-gradient(rgba(0,0,0,${alpha}) 1px, transparent 1px),
        linear-gradient(90deg, rgba(0,0,0,${alpha}) 1px, transparent 1px),
        ${backgroundColor}
      `;
    case 'diagonal':
      return `
        repeating-linear-gradient(
          45deg,
          transparent,
          transparent 8px,
          rgba(0,0,0,${alpha}) 8px,
          rgba(0,0,0,${alpha}) 9px
        ),
        ${backgroundColor}
      `;
    case 'waves':
      return `
        repeating-linear-gradient(
          90deg,
          transparent 0px,
          transparent 15px,
          rgba(0,0,0,${alpha}) 15px,
          rgba(0,0,0,${alpha}) 17px
        ),
        ${backgroundColor}
      `;
    default:
      return backgroundColor;
  }
};

export const getBackgroundSize = (pattern: string): string => {
  switch (pattern) {
    case 'dots':
      return '20px 20px';
    case 'grid':
      return '20px 20px, 20px 20px';
    case 'diagonal':
      return 'auto';
    case 'waves':
      return 'auto';
    case 'none':
    default:
      return 'auto';
  }
};

export const getShadowStyle = (intensity: string | number): string => {
  // Handle numeric values from sliders
  if (!isNaN(Number(intensity))) {
    const numIntensity = Number(intensity);
    if (numIntensity <= 1) return '0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06)';
    if (numIntensity <= 2) return '0 4px 6px rgba(0, 0, 0, 0.1), 0 2px 4px rgba(0, 0, 0, 0.06)';
    return '0 10px 15px rgba(0, 0, 0, 0.1), 0 4px 6px rgba(0, 0, 0, 0.05)';
  }
  
  // Handle string values
  switch (intensity) {
    case 'light':
      return '0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06)';
    case 'medium':
      return '0 4px 6px rgba(0, 0, 0, 0.1), 0 2px 4px rgba(0, 0, 0, 0.06)';
    case 'strong':
      return '0 10px 15px rgba(0, 0, 0, 0.1), 0 4px 6px rgba(0, 0, 0, 0.05)';
    default:
      return '0 4px 6px rgba(0, 0, 0, 0.1), 0 2px 4px rgba(0, 0, 0, 0.06)';
  }
};



export const getFontSize = (fontSize: string | number): string => {
  // Handle numeric pixel values directly from sliders
  if (!isNaN(Number(fontSize))) {
    return `${fontSize}px`;
  }
  
  // Handle string values
  switch (fontSize) {
    case 'small':
      return '14px';
    case 'medium':
      return '16px';
    case 'large':
      return '18px';
    case 'xl':
      return '20px';
    default:
      return '16px';
  }
};

export const getFontWeight = (fontWeight: string | number): number => {
  // Handle numeric values directly from sliders
  if (!isNaN(Number(fontWeight))) {
    return Number(fontWeight);
  }
  
  // Handle string values
  switch (fontWeight) {
    case 'normal':
      return 400;
    case 'medium':
      return 500;
    case 'semibold':
      return 600;
    case 'bold':
      return 700;
    default:
      return 400;
  }
};

 