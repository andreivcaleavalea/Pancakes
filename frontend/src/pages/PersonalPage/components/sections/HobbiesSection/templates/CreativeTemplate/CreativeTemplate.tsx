import React from 'react';
import { Card, Typography } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import type { AdvancedSectionSettings } from '../../../../../../../services/personalPageService';
import './CreativeTemplate.scss';

const { Title, Text } = Typography;

interface CreativeTemplateProps {
  hobbies: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
  editMode?: boolean;
}

const CreativeTemplate: React.FC<CreativeTemplateProps> = ({
  hobbies,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
  advancedSettings,
  editMode = true,
}) => {
  // Build card styles with advanced settings overrides
  const getCardStyles = () => {
    const defaultStyles = {
      marginBottom: '32px',
      borderRadius: '16px',
      position: 'relative' as const,
    };

    if (!advancedSettings) return defaultStyles;

    const { layout, background, styling } = advancedSettings;

    // Generate CSS custom properties for advanced settings
    const cssCustomProperties = {
      '--advanced-background': (background.color || background.pattern !== 'none') ? 
        getBackgroundWithPattern(background.color || '#ffffff', background.pattern, background.opacity) :
        undefined,
      '--advanced-background-size': getBackgroundSize(background.pattern),
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

    // Add animation and transition as regular inline styles
    const finalStyles = {
      ...cssCustomProperties,
            position: 'relative' as const,
      transition: 'none', // Animation disabled
    };

    return finalStyles;
  };

  // Build content styles
  const getContentStyles = () => {
    if (!advancedSettings) return {};
    return {
      padding: '32px' // Default padding
    };
  };

  // Build typography styles
  const getTypographyStyles = () => {
    if (!advancedSettings) return {};
    const { typography } = advancedSettings;
    return {
      fontSize: typography.fontSize ? getFontSize(typography.fontSize) : undefined,
      color: typography.fontColor || undefined,
      fontWeight: typography.fontWeight ? getFontWeight(typography.fontWeight) : undefined,
    };
  };

  const cardStyles = getCardStyles();
  const cardClassName = advancedSettings ? 'creative-template creative-template--custom' : 'creative-template';

  return (
    <Card 
      key="hobbies" 
      className={cardClassName}
      style={cardStyles}
    >
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
        editMode={editMode}
      />
      
      <Title level={2} className="creative-template__title" style={{ color: sectionPrimaryColor }}>
        Hobbies
      </Title>
       
      <div className="creative-template__container">
        {hobbies.map((hobby: any, index: number) => (
          <div 
            key={index} 
            className="creative-template__hobby-card"
            style={{
              background: `${sectionPrimaryColor}15`,
              border: `2px solid ${sectionPrimaryColor}30`
            }}
          >
            <Text className="creative-template__hobby-name" style={{ color: sectionPrimaryColor }}>
              {hobby.name}
            </Text>
          </div>
        ))}
      </div>
    </Card>
  );
};

export default CreativeTemplate; 