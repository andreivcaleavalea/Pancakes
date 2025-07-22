import React from 'react';
import { Card, Avatar, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './CreativeTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface CreativeTemplateProps {
  user: any;
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const CreativeTemplate: React.FC<CreativeTemplateProps> = ({
  user,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card 
      key="personal" 
      className="creative-template"
      style={{
        background: `linear-gradient(45deg, ${sectionPrimaryColor}05, ${sectionPrimaryColor}15)`,
      }}
    >
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />

      {/* Creative Background Pattern */}
      <div 
        className="creative-template__bg-pattern"
        style={{
          background: `radial-gradient(circle, ${sectionPrimaryColor}20, transparent)`,
        }}
      />

      <div className="creative-template__content">
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
              <Title level={2} className="creative-template__name">
                {user.name}
              </Title>

              <div className="creative-template__contact-tags">
                <div 
                  className="creative-template__contact-tag"
                  style={{
                    background: `${sectionPrimaryColor}15`,
                    border: `2px solid ${sectionPrimaryColor}30`,
                    color: sectionPrimaryColor,
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
                    }}
                  >
                    <Text>📞 {user.phoneNumber}</Text>
                  </div>
                )}
              </div>

              {user.bio && (
                <Paragraph className="creative-template__bio">
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