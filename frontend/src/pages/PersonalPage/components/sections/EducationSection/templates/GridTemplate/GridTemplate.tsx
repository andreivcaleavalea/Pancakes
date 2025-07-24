import React from 'react';
import { Card, Typography, Row, Col, Tag } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import './GridTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AdvancedSectionSettings {
  layout: any;
  background: any;
  typography: any;
  styling: any;
}

interface GridTemplateProps {
  educationData: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
}

const GridTemplate: React.FC<GridTemplateProps> = ({
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


    const finalStyles = {
      ...cssCustomProperties,
      position: 'relative' as const,
      transition: 'none', 
      animation: undefined, 
    };

    return finalStyles;
  };


  const getContentStyles = () => {
    if (!advancedSettings) return {};
    return {
      padding: '32px', // Default padding
    };
  };


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
  const cardClassName = advancedSettings ? 'grid-template grid-template--custom' : 'grid-template';

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
      
      <Title level={2} className="grid-template__title" style={{ color: sectionPrimaryColor }}>
        📋 Grid Layout
      </Title>
      
      <Row gutter={[24, 24]}>
        {educationData.map((edu: any, index: number) => (
          <Col key={index} xs={24} sm={12} lg={8}>
            <div 
              className="grid-template__card"
              style={{
                border: `1px solid ${sectionPrimaryColor}20`
              }}
            >
              <div className="grid-template__icon-container">
                <div 
                  className="grid-template__icon"
                  style={{
                    background: `${sectionPrimaryColor}15`
                  }}
                >
                  🎓
                </div>
              </div>
              
              <Title level={5} className="grid-template__institution" style={{ color: sectionPrimaryColor }}>
                {edu.institution}
              </Title>
              
              <Text strong className="grid-template__degree">
                {edu.degree}
              </Text>
              
              <Text className="grid-template__specialization">
                {edu.specialization}
              </Text>
              
              <div className="grid-template__dates">
                <Tag color={sectionPrimaryColor}>
                  {edu.startDate} - {edu.endDate || 'Present'}
                </Tag>
              </div>
              
              {edu.description && (
                <Paragraph className="grid-template__description">
                  {edu.description.length > 60 ? edu.description.substring(0, 60) + '...' : edu.description}
                </Paragraph>
              )}
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  );
};

export default GridTemplate; 