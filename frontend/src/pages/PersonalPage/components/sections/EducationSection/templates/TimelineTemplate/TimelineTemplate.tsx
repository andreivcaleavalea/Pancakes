import React from 'react';
import { Card, Typography } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import './TimelineTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AdvancedSectionSettings {
  layout: any;
  background: any;
  typography: any;
  styling: any;
}

interface TimelineTemplateProps {
  educationData: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
  editMode?: boolean;
}

const TimelineTemplate: React.FC<TimelineTemplateProps> = ({
  educationData,
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

    // Add transition (no animation)
    const finalStyles = {
      ...cssCustomProperties,
      position: 'relative' as const,
      transition: 'none', // Animation disabled
      animation: undefined, // Animation disabled
    };

    return finalStyles;
  };

  // Build content styles
  const getContentStyles = () => {
    if (!advancedSettings) return {};
    return {
      padding: '32px', // Default padding
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
  const cardClassName = advancedSettings ? 'timeline-template timeline-template--custom' : 'timeline-template';

  return (
    <Card 
      key="education" 
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
      
      <Title level={2} style={{ color: sectionPrimaryColor }}>Education</Title>
       
      {educationData.map((edu: any, index: number) => (
        <div key={index} style={{ 
          padding: '16px 0', 
          borderBottom: index < educationData.length - 1 ? `1px solid ${sectionPrimaryColor}20` : 'none' 
        }}>
          <Text strong style={{ fontSize: '16px' }}>{edu.institution}</Text>
          <br />
          <Text>{edu.degree} in {edu.field}</Text>
          <br />
          <Text type="secondary">{edu.startDate} - {edu.endDate || 'Present'}</Text>
          {edu.description && (
            <Paragraph style={{ marginTop: '8px', fontSize: '14px', color: '#666' }}>
              {edu.description}
            </Paragraph>
          )}
        </div>
      ))}
    </Card>
  );
};

export default TimelineTemplate; 