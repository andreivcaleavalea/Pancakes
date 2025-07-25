import React from 'react';
import { Card, Avatar, Typography, Row, Col, Tag } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import './ModernTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AdvancedSectionSettings {
  layout: any;
  background: any;
  typography: any;
  styling: any;
}

interface ModernTemplateProps {
  user: any;
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
}

const ModernTemplate: React.FC<ModernTemplateProps> = ({
  user,
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
      '--advanced-margin-top': `${layout.margin}${typeof layout.margin === 'number' ? 'px' : ''}`,
      '--advanced-margin-bottom': `${layout.margin}${typeof layout.margin === 'number' ? 'px' : ''}`,
      '--advanced-margin-left': layout.fullscreen ? 'calc(-50vw + 50%)' : '0px',
      '--advanced-margin-right': layout.fullscreen ? 'calc(-50vw + 50%)' : '0px',
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
  const cardClassName = advancedSettings ? 'modern-template modern-template--custom' : 'modern-template';

  return (
    <Card 
      key="personal" 
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
      
      {/* Modern geometric pattern */}
      <div 
        className="modern-template__header-line"
        style={{
          background: `linear-gradient(90deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80, ${sectionPrimaryColor}60)`
        }}
      />
      
      <div className="modern-template__content" style={getContentStyles()}>
        <Row gutter={[32, 24]}>
          <Col xs={24} sm={6}>
            <div className="modern-template__avatar-container">
              <div className="modern-template__avatar-wrapper">
                <Avatar
                  size={90}
                  src={user.avatar ? `data:${user.avatar.type};base64,${user.avatar.data}` : undefined}
                  className="modern-template__avatar"
                  style={{ 
                    border: `3px solid ${sectionPrimaryColor}20`
                  }}
                />
                <div className="modern-template__status-indicator" />
              </div>
            </div>
          </Col>
          <Col xs={24} sm={18}>
            <div>
              <Title level={2} className="modern-template__name" style={getTypographyStyles()}>
                {user.name}
              </Title>
              
              <Text className="modern-template__title" style={getTypographyStyles()}>
                {user.title || 'Professional Profile'}
              </Text>
              
              <div className="modern-template__contact-grid">
                <div 
                  className="modern-template__contact-icon"
                  style={{
                    background: `${sectionPrimaryColor}15`
                  }}
                >
                  <span>📧</span>
                </div>
                <Text className="modern-template__contact-text" style={getTypographyStyles()}>{user.email}</Text>
                
                {user.phoneNumber && (
                  <>
                    <div 
                      className="modern-template__contact-icon"
                      style={{
                        background: `${sectionPrimaryColor}15`
                      }}
                    >
                      <span>📞</span>
                    </div>
                    <Text className="modern-template__contact-text" style={getTypographyStyles()}>{user.phoneNumber}</Text>
                  </>
                )}
              </div>
              
              {user.bio && (
                <Paragraph 
                  className="modern-template__bio"
                  style={{
                    borderLeft: `3px solid ${sectionPrimaryColor}30`,
                    ...getTypographyStyles()
                  }}
                >
                  {user.bio}
                </Paragraph>
              )}
            </div>
          </Col>
        </Row>
      </div>
    </Card>
  );
};

export default ModernTemplate; 