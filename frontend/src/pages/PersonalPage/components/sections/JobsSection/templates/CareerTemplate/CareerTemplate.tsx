import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import './CareerTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AdvancedSectionSettings {
  layout: any;
  background: any;
  typography: any;
  styling: any;
  spacing: any;
  animation: any;
}

interface CareerTemplateProps {
  jobs: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
  editMode?: boolean;
}

const CareerTemplate: React.FC<CareerTemplateProps> = ({
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
  const cardClassName = advancedSettings ? 'career-template career-template--custom' : 'career-template';

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
      
      <Title level={2} className="career-template__title" style={{ color: sectionPrimaryColor }}>
        Jobs
      </Title>
      
      <div className="career-template__timeline">
        {/* Career timeline line */}
        <div 
          className="career-template__timeline-line"
          style={{
            background: `linear-gradient(to bottom, ${sectionPrimaryColor}, ${sectionPrimaryColor}50)`
          }}
        />

        {jobs.map((job: any, index: number) => (
          <div key={index} className="career-template__job-item">
            {/* Career milestone */}
            <div 
              className="career-template__milestone"
              style={{ background: sectionPrimaryColor }}
            />

            <div 
              className="career-template__job-card"
              style={{
                border: `1px solid ${sectionPrimaryColor}20`
              }}
            >
              <Row gutter={[16, 12]}>
                <Col xs={24} sm={18}>
                  <Title level={4} className="career-template__job-title" style={{ color: sectionPrimaryColor }}>
                    {job.position}
                  </Title>
                  <Text strong className="career-template__company">{job.company}</Text>
                  <br />
                  {job.location && (
                    <>
                      <Text className="career-template__location">📍 {job.location}</Text>
                      <br />
                    </>
                  )}
                  <Text type="secondary" className="career-template__dates">
                    {job.startDate} - {job.endDate || 'Present'}
                  </Text>
                  {job.description && (
                    <Paragraph className="career-template__description">
                      {job.description}
                    </Paragraph>
                  )}
                </Col>
                <Col xs={24} sm={6}>
                  <div className="career-template__icon-container">
                    <div 
                      className="career-template__icon"
                      style={{
                        background: `${sectionPrimaryColor}15`
                      }}
                    >
                      💼
                    </div>
                  </div>
                </Col>
              </Row>
            </div>
          </div>
        ))}
      </div>
    </Card>
  );
};

export default CareerTemplate; 