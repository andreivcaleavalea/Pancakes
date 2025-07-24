import { useMemo } from 'react';
import type { AdvancedSectionSettings } from '../../../services/personalPageService';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../utils/templateUtils';

interface UseAdvancedStylesReturn {
  cardStyles: React.CSSProperties;
  cardClassName: string;
  getContentStyles: () => React.CSSProperties;
  getTypographyStyles: () => React.CSSProperties;
}

export const useAdvancedStyles = (
  advancedSettings: AdvancedSectionSettings | undefined,
  baseClassName: string,
  defaultStyles?: React.CSSProperties
): UseAdvancedStylesReturn => {
  
  const cardStyles = useMemo(() => {
    const defaults = {
      marginBottom: '32px',
      borderRadius: '16px',
      position: 'relative' as const,
      ...defaultStyles,
    };

    if (!advancedSettings) return defaults;

    const { layout, background, styling } = advancedSettings;

    // Generate CSS custom properties for advanced settings
    const safeColor = background.color && typeof background.color === 'string' ? background.color : '';
    const safePattern = background.pattern && typeof background.pattern === 'string' ? background.pattern : 'none';
    const safeOpacity = background.opacity && typeof background.opacity === 'number' ? background.opacity : 1.0;
    
    const hasBackground = safeColor && safeColor.trim() !== '';
    const hasPattern = safePattern && safePattern !== 'none';
    
    const cssCustomProperties = {
      '--advanced-background': (hasBackground || hasPattern) ? 
        getBackgroundWithPattern(safeColor, safePattern, safeOpacity) :
        undefined,
      '--advanced-background-size': getBackgroundSize(safePattern),
      '--advanced-border-radius': styling.roundCorners ? styling.borderRadius : '0px',
      '--advanced-box-shadow': styling.shadow ? getShadowStyle(styling.shadowIntensity) : 'none',
      '--advanced-margin-bottom': `${layout.margin}${typeof layout.margin === 'number' ? 'px' : ''}`,
      '--advanced-margin-left': layout.fullscreen ? 'calc(-50vw + 50%)' : `${layout.margin}${typeof layout.margin === 'number' ? 'px' : ''}`,
      '--advanced-margin-right': layout.fullscreen ? 'calc(-50vw + 50%)' : `${layout.margin}${typeof layout.margin === 'number' ? 'px' : ''}`,
      '--advanced-border': styling.border.enabled 
        ? `${styling.border.width} ${styling.border.style} ${styling.border.color}`
        : 'none',
      '--advanced-overflow': 'hidden',
      '--advanced-width': layout.fullscreen ? '100vw' : 'auto',
    } as React.CSSProperties;

    // Add transition (no animation)
    const finalStyles = {
      ...cssCustomProperties,
      position: 'relative' as const,
      transition: 'none', // Animation disabled
      animation: undefined, // Animation disabled
    };

    return finalStyles;
  }, [advancedSettings, defaultStyles]);

  const cardClassName = useMemo(() => {
    return advancedSettings ? `${baseClassName} ${baseClassName}--custom` : baseClassName;
  }, [advancedSettings, baseClassName]);

  const getContentStyles = useMemo(() => {
    return () => {
      if (!advancedSettings) return {};
      return {
        padding: '32px', // Default padding
      };
    };
  }, [advancedSettings]);

  const getTypographyStyles = useMemo(() => {
    return () => {
      if (!advancedSettings) return {};
      const { typography } = advancedSettings;
      return {
        fontSize: typography.fontSize ? getFontSize(typography.fontSize) : undefined,
        color: typography.fontColor || undefined,
        fontWeight: typography.fontWeight ? getFontWeight(typography.fontWeight) : undefined,
      };
    };
  }, [advancedSettings]);

  return {
    cardStyles,
    cardClassName,
    getContentStyles,
    getTypographyStyles,
  };
}; 