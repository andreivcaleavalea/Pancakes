import React from 'react';
import { Card, Typography, Row, Col, Tag } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import './JourneyTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AdvancedSectionSettings {
  layout: any;
  background: any;
  typography: any;
  styling: any;
}

interface JourneyTemplateProps {
  educationData: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
}

const JourneyTemplate: React.FC<JourneyTemplateProps> = ({
  educationData,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
  advancedSettings,
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
  const cardClassName = advancedSettings ? 'journey-template journey-template--custom' : 'journey-template';

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
      />
      
      <Title level={2} className="journey-template__title" style={{ color: sectionPrimaryColor }}>
        🛤️ Education Journey
      </Title>
      
      <div className="journey-template__container">
        {educationData.map((edu: any, index: number) => (
          <div key={index} className="journey-template__item">
            {/* Journey Line */}
            {index < educationData.length - 1 && (
              <div 
                className="journey-template__line"
                style={{
                  background: `linear-gradient(to bottom, ${sectionPrimaryColor}, ${sectionPrimaryColor}50)`
                }} 
              />
            )}
            
            <Row align="middle" gutter={[24, 24]}>
              <Col xs={24} md={12} style={{ order: index % 2 === 0 ? 1 : 2 }}>
                <div 
                  className="journey-template__card"
                  style={{
                    border: `2px solid ${sectionPrimaryColor}20`
                  }}
                >
                  <div 
                    className="journey-template__date-badge"
                    style={{
                      background: sectionPrimaryColor
                    }}
                  >
                    {edu.startDate} - {edu.endDate || 'Present'}
                  </div>
                  
                  <Title level={4} className="journey-template__institution" style={{ color: sectionPrimaryColor }}>
                    {edu.institution}
                  </Title>
                  <Text strong className="journey-template__degree">{edu.degree}</Text>
                  <br />
                  <Text className="journey-template__specialization">{edu.specialization}</Text>
                  
                  {edu.description && (
                    <Paragraph className="journey-template__description">
                      {edu.description}
                    </Paragraph>
                  )}
                </div>
              </Col>
              
              <Col xs={24} md={12} className="journey-template__icon-col" style={{ order: index % 2 === 0 ? 2 : 1 }}>
                <div 
                  className="journey-template__icon"
                  style={{
                    background: `linear-gradient(45deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`
                  }}
                >
                  🎓
                </div>
              </Col>
            </Row>
          </div>
        ))}
      </div>
    </Card>
  );
};

export default JourneyTemplate; 