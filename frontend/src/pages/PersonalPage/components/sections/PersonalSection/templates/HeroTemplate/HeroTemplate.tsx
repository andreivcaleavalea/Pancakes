import React from 'react';
import { Card, Avatar, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { useAdvancedStyles } from '../../../../../hooks/useAdvancedStyles';
import type { PersonalTemplateProps } from '../../../../types';
import './HeroTemplate.scss';

const { Title, Text, Paragraph } = Typography;

const HeroTemplate: React.FC<PersonalTemplateProps> = ({
  user,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
  advancedSettings,
  editMode = true,
}) => {
  // Use shared hook instead of duplicated code
  const { cardStyles, cardClassName, getContentStyles, getTypographyStyles } = useAdvancedStyles(
    advancedSettings,
    'hero-template'
  );

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
      
      <div style={getContentStyles()}>
        <Row align="middle" gutter={32}>
          <Col xs={24} md={8} style={{ textAlign: 'center' }}>
            <Avatar
              size={120}
              src={user.avatar ? `https://api.dicebear.com/7.x/avataaars/svg?seed=${user.avatar}` : undefined}
              className="hero-template__avatar"
              style={{ 
                marginBottom: '16px', 
                border: `3px solid ${sectionPrimaryColor}` 
              }}
            />
          </Col>
          <Col xs={24} md={16}>
            <Title level={1} style={{ 
              ...getTypographyStyles(), 
              color: sectionPrimaryColor, 
              marginBottom: '8px' 
            }}>
              {user.name}
            </Title>
            <Title level={3} style={{ 
              ...getTypographyStyles(), 
              marginBottom: '16px', 
              fontWeight: 400 
            }}>
              {user.email}
            </Title>
            {user.location && (
              <Text style={{ 
                ...getTypographyStyles(), 
                display: 'block', 
                marginBottom: '8px' 
              }}>
                📍 {user.location}
              </Text>
            )}
            {user.phoneNumber && (
              <Text style={{ 
                ...getTypographyStyles(), 
                display: 'block', 
                marginBottom: '16px' 
              }}>
                📞 {user.phoneNumber}
              </Text>
            )}
            {user.bio && (
              <Paragraph style={{ 
                ...getTypographyStyles(), 
                fontSize: '16px', 
                lineHeight: '1.6' 
              }}>
                {user.bio}
              </Paragraph>
            )}
          </Col>
        </Row>
      </div>
    </Card>
  );
};

export default HeroTemplate; 