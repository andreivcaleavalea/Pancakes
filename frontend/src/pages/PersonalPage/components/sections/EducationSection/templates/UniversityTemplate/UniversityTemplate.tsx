import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import './UniversityTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AdvancedSectionSettings {
  layout: any;
  background: any;
  typography: any;
  styling: any;
}

interface UniversityTemplateProps {
  educationData: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
  editMode?: boolean;
}

const UniversityTemplate: React.FC<UniversityTemplateProps> = ({
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
  const cardClassName = advancedSettings ? 'university-template university-template--custom' : 'university-template';

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
      
      {/* University Header */}
      <div 
        className="university-template__header"
        style={{
          background: `linear-gradient(135deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}dd)`
        }}
      >
        <Title level={2} className="university-template__title">
          Education
        </Title>
        <Text className="university-template__subtitle">
          Academic Excellence Journey
        </Text>
      </div>
      
      <div className="university-template__content">
        {educationData.map((edu: any, index: number) => (
          <div 
            key={index} 
            className="university-template__item"
            style={{
              border: `1px solid ${sectionPrimaryColor}15`
            }}
          >
            {/* University Badge */}
            <div 
              className="university-template__badge"
              style={{
                background: sectionPrimaryColor
              }}
            >
              #{index + 1}
            </div>
            
            <Row gutter={[24, 16]}>
              <Col xs={24} sm={16}>
                <div>
                  <Title level={3} className="university-template__institution" style={{ color: sectionPrimaryColor }}>
                    {edu.institution}
                  </Title>
                  
                  <div 
                    className="university-template__degree-card"
                    style={{
                      background: `${sectionPrimaryColor}08`,
                      borderLeft: `4px solid ${sectionPrimaryColor}`
                    }}
                  >
                    <Text strong className="university-template__degree">
                      {edu.degree}
                    </Text>
                    <br />
                    <Text className="university-template__specialization" style={{ color: sectionPrimaryColor }}>
                      Major: {edu.specialization}
                    </Text>
                  </div>
                  
                  {edu.description && (
                    <Paragraph className="university-template__description">
                      {edu.description}
                    </Paragraph>
                  )}
                </div>
              </Col>
              
              <Col xs={24} sm={8}>
                <div className="university-template__icon-container">
                  <div 
                    className="university-template__icon"
                    style={{
                      background: `linear-gradient(135deg, ${sectionPrimaryColor}15, ${sectionPrimaryColor}25)`
                    }}
                  >
                    🎓
                  </div>
                  <div 
                    className="university-template__date-badge"
                    style={{
                      background: `${sectionPrimaryColor}10`
                    }}
                  >
                    <Text className="university-template__date-text" style={{ color: sectionPrimaryColor }}>
                      {edu.startDate} - {edu.endDate || 'Present'}
                    </Text>
                  </div>
                </div>
              </Col>
            </Row>
          </div>
        ))}
      </div>
    </Card>
  );
};

export default UniversityTemplate; 