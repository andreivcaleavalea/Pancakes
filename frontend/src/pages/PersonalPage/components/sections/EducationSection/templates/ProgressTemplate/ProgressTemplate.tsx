import React from 'react';
import { Card, Typography, Progress, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import './ProgressTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AdvancedSectionSettings {
  layout: any;
  background: any;
  typography: any;
  styling: any;
}

interface ProgressTemplateProps {
  educationData: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
  editMode?: boolean;
}

const ProgressTemplate: React.FC<ProgressTemplateProps> = ({
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
  const cardClassName = advancedSettings ? 'progress-template progress-template--custom' : 'progress-template';

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
      
      <Title level={2} className="progress-template__title" style={{ color: sectionPrimaryColor }}>
        Education
      </Title>
      
      <div className="progress-template__container">
        <div className="progress-template__timeline-container">
          <div 
            className="progress-template__timeline-line"
            style={{
              background: `linear-gradient(to bottom, ${sectionPrimaryColor}, ${sectionPrimaryColor}40)`
            }}
          />
          
          {educationData.map((edu: any, index: number) => (
            <div key={index} className="progress-template__item">
              {/* Progress Dot */}
              <div 
                className="progress-template__progress-dot"
                style={{ background: sectionPrimaryColor }}
              />
              
              {/* Progress Indicator */}
              <div 
                className="progress-template__progress-indicator"
                style={{
                  background: `${sectionPrimaryColor}15`
                }}
              >
                <Text className="progress-template__percentage" style={{ color: sectionPrimaryColor }}>
                  {Math.round(((index + 1) / educationData.length) * 100)}%
                </Text>
              </div>
              
              <div 
                className="progress-template__card"
                style={{
                  border: `1px solid ${sectionPrimaryColor}20`
                }}
              >
                <Row gutter={[16, 12]}>
                  <Col xs={24} sm={18}>
                    <Title level={4} className="progress-template__institution" style={{ color: sectionPrimaryColor }}>
                      {edu.institution}
                    </Title>
                    <Text strong className="progress-template__degree">{edu.degree}</Text>
                    <br />
                    <Text className="progress-template__specialization">{edu.specialization}</Text>
                    
                    {edu.description && (
                      <Paragraph className="progress-template__description">
                        {edu.description}
                      </Paragraph>
                    )}
                  </Col>
                  
                  <Col xs={24} sm={6}>
                    <div className="progress-template__dates-container">
                      <div 
                        className="progress-template__date-badge progress-template__date-badge--start"
                        style={{
                          background: `${sectionPrimaryColor}10`
                        }}
                      >
                        <Text className="progress-template__date-text" style={{ color: sectionPrimaryColor }}>
                          {edu.startDate}
                        </Text>
                      </div>
                      <br />
                      <div 
                        className="progress-template__date-badge progress-template__date-badge--end"
                        style={{
                          background: `${sectionPrimaryColor}20`
                        }}
                      >
                        <Text className="progress-template__date-text" style={{ color: sectionPrimaryColor }}>
                          {edu.endDate || 'Present'}
                        </Text>
                      </div>
                    </div>
                  </Col>
                </Row>
              </div>
            </div>
          ))}
        </div>
      </div>
    </Card>
  );
};

export default ProgressTemplate; 