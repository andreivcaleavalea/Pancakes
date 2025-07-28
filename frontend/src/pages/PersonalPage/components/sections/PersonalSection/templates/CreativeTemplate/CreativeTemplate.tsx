import React from 'react';
import { Card, Avatar, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import './CreativeTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AdvancedSectionSettings {
  layout: any;
  background: any;
  typography: any;
  styling: any;
  spacing: any;
  animation: any;
}

interface CreativeTemplateProps {
  user: any;
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
  editMode?: boolean;
}

const CreativeTemplate: React.FC<CreativeTemplateProps> = ({
  user,
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
      background: `linear-gradient(45deg, ${sectionPrimaryColor}05, ${sectionPrimaryColor}15)`,
      marginBottom: '32px',
      position: 'relative' as const,
      borderRadius: '24px',
      overflow: 'hidden' as const,
      border: 'none',
      boxShadow: '0 15px 35px rgba(0,0,0,0.1)',
    };

    if (!advancedSettings) {
      return defaultStyles;
    }

    const { layout, background, styling } = advancedSettings;

    // Generate CSS custom properties for advanced settings
    const cssCustomProperties = {
      '--advanced-background': (background.color || background.pattern !== 'none') ? 
        getBackgroundWithPattern(background.color || '#ffffff', background.pattern, background.opacity) :
        defaultStyles.background,
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

    // Add animation and transition as regular inline styles (these work fine)
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
    if (!advancedSettings) return { padding: '40px' };
    return {
      padding: '32px', // Default padding
      position: 'relative' as const,
      zIndex: 1,
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

  // Use different className when advanced settings are active to avoid SCSS conflicts
  const cardClassName = advancedSettings ? 'creative-template creative-template--custom' : 'creative-template';

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

      {/* Creative Background Pattern - only show if no custom background */}
      {(!advancedSettings?.background.color && (!advancedSettings?.background.pattern || advancedSettings?.background.pattern === 'none')) && (
        <div 
          className="creative-template__bg-pattern"
          style={{
            background: `radial-gradient(circle, ${sectionPrimaryColor}20, transparent)`,
          }}
        />
      )}

      <div className="creative-template__content" style={getContentStyles()}>
        <Row align="middle" gutter={[32, 24]}>
          <Col xs={24} md={8}>
            <div className="creative-template__avatar-container">
              <div className="creative-template__avatar-wrapper">
                {/* Rotating Ring Animation */}
                <div 
                  className="creative-template__rotating-ring"
                  style={{
                    background: `conic-gradient(${sectionPrimaryColor}60, transparent, ${sectionPrimaryColor}60, transparent, ${sectionPrimaryColor}60)`,
                  }}
                />

                {/* Pulsing Inner Ring */}
                <div 
                  className="creative-template__pulsing-ring"
                  style={{
                    background: `linear-gradient(45deg, ${sectionPrimaryColor}30, transparent, ${sectionPrimaryColor}30)`,
                  }}
                />

                <Avatar
                  size={100}
                  src={user.avatar ? `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${user.avatar}` : undefined}
                  className="creative-template__avatar"
                />
              </div>
            </div>
          </Col>
          
          <Col xs={24} md={16}>
            <div>
              <Title level={2} className="creative-template__name" style={getTypographyStyles()}>
                {user.name}
              </Title>

              <div className="creative-template__contact-tags">
                <div 
                  className="creative-template__contact-tag"
                  style={{
                    background: `${sectionPrimaryColor}15`,
                    border: `2px solid ${sectionPrimaryColor}30`,
                    color: sectionPrimaryColor,
                    ...getTypographyStyles(),
                  }}
                >
                  <Text>📧 {user.email}</Text>
                </div>
                {user.phoneNumber && (
                  <div 
                    className="creative-template__contact-tag"
                    style={{
                      background: `${sectionPrimaryColor}15`,
                      border: `2px solid ${sectionPrimaryColor}30`,
                      color: sectionPrimaryColor,
                      ...getTypographyStyles(),
                    }}
                  >
                    <Text>📞 {user.phoneNumber}</Text>
                  </div>
                )}
              </div>

              {user.bio && (
                <Paragraph className="creative-template__bio" style={getTypographyStyles()}>
                  "{user.bio}"
                </Paragraph>
              )}
            </div>
          </Col>
        </Row>
      </div>
    </Card>
  );
};

export default CreativeTemplate; 