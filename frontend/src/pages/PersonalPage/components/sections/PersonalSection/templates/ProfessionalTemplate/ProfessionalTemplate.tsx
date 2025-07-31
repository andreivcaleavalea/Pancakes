import React from 'react';
import { Card, Avatar, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import './ProfessionalTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AdvancedSectionSettings {
  layout: any;
  background: any;
  typography: any;
  styling: any;
}

interface ProfessionalTemplateProps {
  user: any;
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
  editMode: boolean;
}

const ProfessionalTemplate: React.FC<ProfessionalTemplateProps> = ({
  user,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
  advancedSettings,
  editMode,
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
  const cardClassName = advancedSettings ? 'professional-template professional-template--custom' : 'professional-template';

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
      
      {/* Professional Header */}
      <div 
        className="professional-template__header"
        style={{
          background: `linear-gradient(135deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}dd)`,
        }}
      >
        <div className="professional-template__avatar-wrapper">
          <Avatar
            size={80}
            src={(() => {
              const imageUrl = user.avatar || user.image;
              if (!imageUrl) return undefined;
              return imageUrl.startsWith('http') ? imageUrl : `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${imageUrl}`;
            })()}
            className="professional-template__avatar"
          />
        </div>
      </div>
      
      <div className="professional-template__content" style={getContentStyles()}>
        <Title level={2} className="professional-template__name" style={getTypographyStyles()}>
          {user.name}
        </Title>
        
        <Text 
          className="professional-template__title"
          style={{ color: sectionPrimaryColor, ...getTypographyStyles() }}
        >
          {user.title || 'Professional'}
        </Text>
        
        <Row gutter={[24, 16]}>
          <Col xs={24} sm={12}>
            <div className="professional-template__contact-item">
              <span 
                className="professional-template__contact-icon"
                style={{ color: sectionPrimaryColor }}
              >
                📧
              </span>
              <Text style={getTypographyStyles()}>{user.email}</Text>
            </div>
          </Col>
          {user.phoneNumber && (
            <Col xs={24} sm={12}>
              <div className="professional-template__contact-item">
                <span 
                  className="professional-template__contact-icon"
                  style={{ color: sectionPrimaryColor }}
                >
                  📞
                </span>
                <Text style={getTypographyStyles()}>{user.phoneNumber}</Text>
              </div>
            </Col>
          )}
        </Row>
        
        {user.bio && (
          <div 
            className="professional-template__bio"
            style={{ borderLeft: `4px solid ${sectionPrimaryColor}` }}
          >
            <Paragraph className="professional-template__bio-text" style={getTypographyStyles()}>
              {user.bio}
            </Paragraph>
          </div>
        )}
      </div>
    </Card>
  );
};

export default ProfessionalTemplate; 