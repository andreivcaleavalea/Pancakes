import React from 'react';
import { Card, Typography, Row, Col, Tag } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import type { AdvancedSectionSettings } from '../../../../../../../services/personalPageService';
import './ExperienceTemplate.scss';

const { Title, Text, Paragraph } = Typography;



interface ExperienceTemplateProps {
  jobs: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
  editMode?: boolean;
}

const ExperienceTemplate: React.FC<ExperienceTemplateProps> = ({
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
  const cardClassName = advancedSettings ? 'experience-template experience-template--custom' : 'experience-template';

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
      
      <Title level={2} style={{ color: sectionPrimaryColor, textAlign: 'center', marginBottom: '32px' }}>
        Jobs
      </Title>
      
      <Row gutter={[24, 24]}>
        {jobs.map((job: any, index: number) => (
          <Col key={index} xs={24} md={12} lg={8}>
            <div style={{
              background: 'linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%)',
              borderRadius: '16px',
              padding: '24px',
              height: '100%',
              border: `2px solid ${sectionPrimaryColor}15`,
              position: 'relative',
              overflow: 'hidden',
              boxShadow: '0 8px 25px rgba(0,0,0,0.08)',
              transition: 'transform 0.3s ease, box-shadow 0.3s ease'
            }}>
              {/* Experience Level Indicator */}
              <div style={{
                position: 'absolute',
                top: 0,
                left: 0,
                width: '100%',
                height: '6px',
                background: `linear-gradient(90deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`
              }} />
              
              {/* Experience Badge */}
              <div style={{
                position: 'absolute',
                top: '12px',
                right: '12px',
                background: sectionPrimaryColor,
                color: 'white',
                padding: '4px 8px',
                borderRadius: '8px',
                fontSize: '10px',
                fontWeight: '600'
              }}>
                #{index + 1}
              </div>
              
              <div style={{ textAlign: 'center', marginBottom: '20px', marginTop: '16px' }}>
                <div style={{
                  background: `${sectionPrimaryColor}15`,
                  borderRadius: '50%',
                  width: '60px',
                  height: '60px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  margin: '0 auto 12px',
                  fontSize: '24px'
                }}>
                  {index === 0 ? '🎯' : index === 1 ? '⭐' : '💼'}
                </div>
              </div>
              
              <Title level={4} style={{ 
                margin: 0, 
                color: sectionPrimaryColor,
                fontSize: '16px',
                textAlign: 'center',
                marginBottom: '8px'
              }}>
                {job.position}
              </Title>
              
              <Text strong style={{ 
                display: 'block',
                textAlign: 'center',
                fontSize: '14px',
                marginBottom: '12px'
              }}>
                {job.company}
              </Text>
              
              {job.location && (
                <Text style={{ 
                  display: 'block',
                  textAlign: 'center',
                  fontSize: '12px',
                  color: '#666',
                  marginBottom: '16px'
                }}>
                  📍 {job.location}
                </Text>
              )}
              
              <div style={{ textAlign: 'center', marginBottom: '16px' }}>
                <Tag color={sectionPrimaryColor}>
                  {job.startDate} - {job.endDate || 'Present'}
                </Tag>
              </div>
              
              {job.description && (
                <Paragraph style={{ 
                  fontSize: '12px', 
                  color: '#666',
                  lineHeight: 1.4,
                  margin: 0,
                  textAlign: 'center'
                }}>
                  {job.description.length > 80 ? job.description.substring(0, 80) + '...' : job.description}
                </Paragraph>
              )}
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  );
};

export default ExperienceTemplate; 