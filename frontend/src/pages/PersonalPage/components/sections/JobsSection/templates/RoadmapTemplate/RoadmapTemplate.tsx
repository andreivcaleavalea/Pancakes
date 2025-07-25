import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import './RoadmapTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AdvancedSectionSettings {
  layout: any;
  background: any;
  typography: any;
  styling: any;
  spacing: any;
  animation: any;
}

interface RoadmapTemplateProps {
  jobs: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
  editMode?: boolean;
}

const RoadmapTemplate: React.FC<RoadmapTemplateProps> = ({
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
      position: 'relative' as const,
      borderRadius: '16px',
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
  const cardClassName = advancedSettings ? 'roadmap-template roadmap-template--custom' : 'roadmap-template';

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
      
      <div style={{ padding: '20px', position: 'relative' }}>
        {/* Roadmap Path */}
        <div style={{
          position: 'absolute',
          top: '80px',
          left: '50%',
          transform: 'translateX(-50%)',
          width: '4px',
          height: `${(jobs.length - 1) * 200 + 100}px`,
          background: `linear-gradient(to bottom, ${sectionPrimaryColor}, ${sectionPrimaryColor}40)`,
          borderRadius: '2px',
          zIndex: 1
        }} />
        
        {jobs.map((job: any, index: number) => (
          <div key={index} style={{ 
            position: 'relative',
            marginBottom: '60px',
            zIndex: 2
          }}>
            {/* Roadmap Milestone */}
            <div style={{
              position: 'absolute',
              left: '50%',
              top: '20px',
              transform: 'translateX(-50%)',
              width: '24px',
              height: '24px',
              background: sectionPrimaryColor,
              borderRadius: '50%',
              border: '6px solid white',
              boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
              zIndex: 3
            }} />
            
            {/* Milestone Number */}
            <div style={{
              position: 'absolute',
              left: '50%',
              top: '-15px',
              transform: 'translateX(-50%)',
              width: '40px',
              height: '40px',
              background: `${sectionPrimaryColor}15`,
              borderRadius: '50%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              border: `2px solid ${sectionPrimaryColor}`,
              zIndex: 4
            }}>
              <Text style={{ fontSize: '12px', color: sectionPrimaryColor, fontWeight: '700' }}>
                {jobs.length - index}
              </Text>
            </div>
            
            <Row gutter={[32, 24]}>
              <Col xs={24} md={12} style={{ order: index % 2 === 0 ? 1 : 2 }}>
                <div style={{
                  background: index % 2 === 0 ? '#fff' : `${sectionPrimaryColor}05`,
                  borderRadius: '16px',
                  padding: '24px',
                  border: `2px solid ${sectionPrimaryColor}20`,
                  position: 'relative',
                  boxShadow: '0 8px 25px rgba(0,0,0,0.08)',
                  marginTop: '40px'
                }}>
                  {/* Achievement Badge */}
                  <div style={{
                    position: 'absolute',
                    top: '-15px',
                    left: '20px',
                    background: index === 0 ? '#52c41a' : sectionPrimaryColor,
                    color: 'white',
                    padding: '6px 12px',
                    borderRadius: '12px',
                    fontSize: '10px',
                    fontWeight: '600'
                  }}>
                    {index === 0 ? '🎯 CURRENT' : '✅ COMPLETED'}
                  </div>
                  
                  <Title level={4} style={{ 
                    color: sectionPrimaryColor, 
                    marginBottom: '8px',
                    fontSize: '18px'
                  }}>
                    {job.position}
                  </Title>
                  
                  <Title level={5} style={{ 
                    color: '#2c3e50', 
                    marginBottom: '12px',
                    fontWeight: '600'
                  }}>
                    {job.company}
                  </Title>
                  
                  {job.location && (
                    <div style={{
                      display: 'flex',
                      alignItems: 'center',
                      marginBottom: '12px'
                    }}>
                      <span style={{ fontSize: '14px', marginRight: '8px' }}>📍</span>
                      <Text style={{ fontSize: '13px', color: '#666' }}>{job.location}</Text>
                    </div>
                  )}
                  
                  <div style={{
                    display: 'flex',
                    gap: '8px',
                    marginBottom: '12px'
                  }}>
                    <div style={{
                      background: `${sectionPrimaryColor}15`,
                      padding: '4px 8px',
                      borderRadius: '6px'
                    }}>
                      <Text style={{ fontSize: '10px', color: sectionPrimaryColor, fontWeight: '600' }}>
                        START: {job.startDate}
                      </Text>
                    </div>
                    <div style={{
                      background: job.endDate ? `${sectionPrimaryColor}25` : '#52c41a15',
                      padding: '4px 8px',
                      borderRadius: '6px'
                    }}>
                      <Text style={{ fontSize: '10px', color: job.endDate ? sectionPrimaryColor : '#52c41a', fontWeight: '600' }}>
                        {job.endDate ? `END: ${job.endDate}` : 'ONGOING'}
                      </Text>
                    </div>
                  </div>
                  
                  {job.description && (
                    <Paragraph style={{ 
                      fontSize: '13px', 
                      color: '#666',
                      lineHeight: 1.5,
                      margin: 0
                    }}>
                      {job.description}
                    </Paragraph>
                  )}
                </div>
              </Col>
              
              <Col xs={24} md={12} style={{ 
                order: index % 2 === 0 ? 2 : 1,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center'
              }}>
                <div style={{
                  textAlign: 'center',
                  padding: '20px'
                }}>
                  <div style={{
                    width: '100px',
                    height: '100px',
                    background: `linear-gradient(135deg, ${sectionPrimaryColor}20, ${sectionPrimaryColor}40)`,
                    borderRadius: '20px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '36px',
                    margin: '0 auto 12px',
                    transform: 'rotate(5deg)',
                    boxShadow: '0 12px 25px rgba(0,0,0,0.1)'
                  }}>
                    {index === 0 ? '🎯' : index === 1 ? '🚀' : '💼'}
                  </div>
                  <Text style={{ 
                    fontSize: '12px', 
                    color: '#666',
                    fontWeight: '500',
                    textTransform: 'uppercase',
                    letterSpacing: '1px'
                  }}>
                    Phase {jobs.length - index}
                  </Text>
                </div>
              </Col>
            </Row>
          </div>
        ))}
      </div>
    </Card>
  );
};

export default RoadmapTemplate; 