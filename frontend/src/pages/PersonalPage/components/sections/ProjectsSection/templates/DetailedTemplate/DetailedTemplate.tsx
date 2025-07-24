import React from 'react';
import { Card, Typography, Row, Col, Tag, Space } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import './DetailedTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AdvancedSectionSettings {
  layout: any;
  background: any;
  typography: any;
  styling: any;
  spacing: any;
  animation: any;
}

interface DetailedTemplateProps {
  projects: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
}

const DetailedTemplate: React.FC<DetailedTemplateProps> = ({
  projects,
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
    const { spacing } = advancedSettings;
    return {
      padding: '32px' // Default padding,
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
  const cardClassName = advancedSettings ? 'detailed-template detailed-template--custom' : 'detailed-template';

  return (
    <Card 
      key="projects" 
      className={cardClassName}
      style={cardStyles}
    >
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="detailed-template__title" style={{ color: sectionPrimaryColor }}>
        📋 Detailed View
      </Title>
       
      {projects.map((project: any, index: number) => (
        <div 
          key={index} 
          className="detailed-template__project-item"
          style={{ 
            border: `1px solid ${sectionPrimaryColor}20`
          }}
        >
          <Row gutter={[16, 16]}>
            <Col xs={24} md={16}>
              <Title level={4} className="detailed-template__project-name" style={{ color: sectionPrimaryColor }}>
                {project.name}
              </Title>
              {project.description && (
                <Paragraph className="detailed-template__project-description">
                  {project.description}
                </Paragraph>
              )}
              {project.technologies && (
                <Text type="secondary" className="detailed-template__project-tech">
                  Technologies: {project.technologies}
                </Text>
              )}
            </Col>
            <Col xs={24} md={8}>
              <div className="detailed-template__links-container">
                {project.demoUrl && (
                  <div className="detailed-template__demo-link">
                    <Text strong>🔗 Demo Available</Text>
                  </div>
                )}
                {project.sourceUrl && (
                  <div className="detailed-template__source-link">
                    <Text strong>💻 Source Code</Text>
                  </div>
                )}
              </div>
            </Col>
          </Row>
        </div>
      ))}
    </Card>
  );
};

export default DetailedTemplate; 