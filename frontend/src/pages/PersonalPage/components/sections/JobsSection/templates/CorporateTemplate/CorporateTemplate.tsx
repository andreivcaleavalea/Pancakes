import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import './CorporateTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AdvancedSectionSettings {
  layout: any;
  background: any;
  typography: any;
  styling: any;
  spacing: any;
  animation: any;
}

interface CorporateTemplateProps {
  jobs: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
  editMode?: boolean;
}

const CorporateTemplate: React.FC<CorporateTemplateProps> = ({
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
      borderRadius: '20px',
      background: 'linear-gradient(135deg, #f8f9fa 0%, #ffffff 100%)',
    };

    if (!advancedSettings) return defaultStyles;

    const { layout, background, styling } = advancedSettings;

    // Generate CSS custom properties for advanced settings
    const cssCustomProperties = {
      '--advanced-background': (background.color || background.pattern !== 'none') ? 
        getBackgroundWithPattern(background.color || '#ffffff', background.pattern, background.opacity) :
        defaultStyles.background,
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
  const cardClassName = advancedSettings ? 'corporate-template corporate-template--custom' : 'corporate-template';

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
      
      {/* Corporate Header */}
      <div style={{
        background: `linear-gradient(135deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}dd)`,
        padding: '32px',
        textAlign: 'center',
        borderRadius: '20px 20px 0 0',
        position: 'relative'
      }}>
        <Title level={2} style={{ color: 'white', margin: 0, fontSize: '28px', fontWeight: '700' }}>
          Jobs
        </Title>
        <Text style={{ color: 'rgba(255,255,255,0.9)', fontSize: '16px', marginTop: '8px' }}>
          Professional Career Journey
        </Text>
      </div>
      
      <div style={{ padding: '32px' }}>
        {jobs.map((job: any, index: number) => (
          <div key={index} style={{
            background: '#fff',
            borderRadius: '16px',
            padding: '28px',
            marginBottom: index < jobs.length - 1 ? '24px' : 0,
            border: `1px solid ${sectionPrimaryColor}15`,
            position: 'relative',
            boxShadow: '0 4px 15px rgba(0,0,0,0.05)'
          }}>
            {/* Corporate Badge */}
            <div style={{
              position: 'absolute',
              top: '-12px',
              right: '20px',
              background: sectionPrimaryColor,
              color: 'white',
              padding: '6px 12px',
              borderRadius: '12px',
              fontSize: '11px',
              fontWeight: '600',
              textTransform: 'uppercase'
            }}>
              Position #{index + 1}
            </div>
            
            <Row gutter={[24, 16]}>
              <Col xs={24} sm={16}>
                <div>
                  <Title level={3} style={{ 
                    color: sectionPrimaryColor, 
                    marginBottom: '8px',
                    fontSize: '20px'
                  }}>
                    {job.position}
                  </Title>
                  
                  <Title level={4} style={{ 
                    color: '#2c3e50', 
                    marginBottom: '12px',
                    fontWeight: '600'
                  }}>
                    {job.company}
                  </Title>
                  
                  {job.location && (
                    <div style={{
                      background: `${sectionPrimaryColor}08`,
                      padding: '8px 12px',
                      borderRadius: '8px',
                      marginBottom: '12px',
                      display: 'inline-block'
                    }}>
                      <Text style={{ fontSize: '13px', color: sectionPrimaryColor, fontWeight: '500' }}>
                        📍 {job.location}
                      </Text>
                    </div>
                  )}
                  
                  {job.description && (
                    <Paragraph style={{ 
                      fontSize: '14px', 
                      color: '#666',
                      lineHeight: 1.6,
                      margin: 0
                    }}>
                      {job.description}
                    </Paragraph>
                  )}
                </div>
              </Col>
              
              <Col xs={24} sm={8}>
                <div style={{ textAlign: 'center' }}>
                  <div style={{
                    background: `linear-gradient(135deg, ${sectionPrimaryColor}15, ${sectionPrimaryColor}25)`,
                    borderRadius: '16px',
                    width: '80px',
                    height: '80px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    margin: '0 auto 12px',
                    fontSize: '28px'
                  }}>
                    🏢
                  </div>
                  <div style={{
                    background: `${sectionPrimaryColor}10`,
                    padding: '8px 12px',
                    borderRadius: '20px',
                    display: 'inline-block'
                  }}>
                    <Text style={{ fontSize: '12px', color: sectionPrimaryColor, fontWeight: '600' }}>
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

export default CorporateTemplate; 