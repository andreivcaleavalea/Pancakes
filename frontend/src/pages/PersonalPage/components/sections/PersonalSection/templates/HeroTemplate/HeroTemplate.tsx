import React from 'react';
import { Card, Avatar, Typography, Row, Col, Space } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getProfilePictureUrl } from '../../../../../../../utils/imageUtils';
import './HeroTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface HeroTemplateProps {
  user: any;
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const HeroTemplate: React.FC<HeroTemplateProps> = ({
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
      className="hero-template"
      style={{
        background: `linear-gradient(135deg, ${sectionPrimaryColor}10, ${sectionPrimaryColor}05)`,
        border: `1px solid ${sectionPrimaryColor}20`,
      }}
    >
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />

      <div className="hero-template__content">
        <Row align="middle" gutter={[32, 24]}>
          <Col xs={24} sm={8} md={6}>
            <div className="hero-template__avatar-container">
              <div 
                className="hero-template__avatar-wrapper"
                style={{
                  background: `linear-gradient(45deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}90)`,
                }}
              >
                <Avatar
                  size={120}
                  src={getProfilePictureUrl(user.avatar)}
                  className="hero-template__avatar"
                />
              </div>
            </div>
          </Col>
          <Col xs={24} sm={16} md={18}>
            <Space direction="vertical" size="large" className="hero-template__info">
              <div>
                <Title level={1} className="hero-template__name">
                  {user.name}
                </Title>
                <div 
                  className="hero-template__contact-card"
                  style={{
                    background: `${sectionPrimaryColor}15`,
                    borderLeft: `4px solid ${sectionPrimaryColor}`
                  }}
                >
                  <Text>
                    📧 {user.email}
                    {user.phoneNumber && <span className="hero-template__phone">📞 {user.phoneNumber}</span>}
                  </Text>
                </div>
              </div>
              {user.bio && (
                <Paragraph className="hero-template__bio">
                  {user.bio}
                </Paragraph>
              )}
            </Space>
          </Col>
        </Row>
      </div>
    </Card>
  );
};

export default HeroTemplate; 