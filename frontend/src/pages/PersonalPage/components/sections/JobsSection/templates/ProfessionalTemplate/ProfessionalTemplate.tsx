import React from 'react';
import { Card, Typography, Row, Col, Space } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import './ProfessionalTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AdvancedSectionSettings {
  layout: any;
  background: any;
  typography: any;
  styling: any;
  spacing: any;
  animation: any;
}

interface ProfessionalTemplateProps {
  jobs: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
  editMode?: boolean;
}

const ProfessionalTemplate: React.FC<ProfessionalTemplateProps> = ({
  jobs,
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
  const cardClassName = advancedSettings ? 'professional-template professional-template--custom' : 'professional-template';

  return (
    <Card 
      key="jobs" 
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
      
      {/* Professional Header */}
      <div 
        className="professional-template__header"
        style={{
          background: `linear-gradient(135deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}dd)`
        }}
      >
        <Title level={2} className="professional-template__title">
          Jobs
        </Title>
      </div>
      
      <div className="professional-template__content">
        {jobs.map((job: any, index: number) => (
          <div 
            key={index} 
            className="professional-template__job-item"
            style={{
              borderBottom: index < jobs.length - 1 ? `1px solid ${sectionPrimaryColor}15` : 'none'
            }}
          >
            <Row gutter={[24, 16]}>
              <Col xs={24} sm={18}>
                <div>
                  <Title level={3} className="professional-template__job-position" style={{ color: sectionPrimaryColor }}>
                    {job.position}
                  </Title>
                  
                  <Text className="professional-template__company" style={{ color: sectionPrimaryColor }}>
                    {job.company}
                  </Text>
                  
                  {job.location && (
                    <div className="professional-template__location-container">
                      <span className="professional-template__location-icon" style={{ color: sectionPrimaryColor }}>📍</span>
                      <Text>{job.location}</Text>
                    </div>
                  )}
                  
                  {job.description && (
                    <div 
                      className="professional-template__description-card"
                      style={{
                        borderLeft: `4px solid ${sectionPrimaryColor}`
                      }}
                    >
                      <Paragraph className="professional-template__description">
                        {job.description}
                      </Paragraph>
                    </div>
                  )}
                </div>
              </Col>
              
              <Col xs={24} sm={6}>
                <div className="professional-template__icon-container">
                  <div 
                    className="professional-template__icon"
                    style={{
                      background: `linear-gradient(135deg, ${sectionPrimaryColor}15, ${sectionPrimaryColor}25)`
                    }}
                  >
                    👔
                  </div>
                  <div 
                    className="professional-template__date-badge"
                    style={{
                      background: `${sectionPrimaryColor}10`
                    }}
                  >
                    <Text className="professional-template__date-text" style={{ color: sectionPrimaryColor }}>
                      {job.startDate} - {job.endDate || 'Present'}
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

export default ProfessionalTemplate; 