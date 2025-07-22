import React from 'react';
import { Card, Avatar, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getProfilePictureUrl } from '../../../../../../../utils/imageUtils';
import './ModernTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface ModernTemplateProps {
  user: any;
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const ModernTemplate: React.FC<ModernTemplateProps> = ({
  user,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="personal" className="modern-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      {/* Modern geometric pattern */}
      <div 
        className="modern-template__header-line"
        style={{
          background: `linear-gradient(90deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80, ${sectionPrimaryColor}60)`
        }}
      />
      
      <div className="modern-template__content">
        <Row gutter={[32, 24]}>
          <Col xs={24} sm={6}>
            <div className="modern-template__avatar-container">
              <div className="modern-template__avatar-wrapper">
                <Avatar
                  size={90}
                  src={getProfilePictureUrl(user.avatar)}
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
              <Title level={2} className="modern-template__name">
                {user.name}
              </Title>
              
              <Text className="modern-template__title">
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
                <Text className="modern-template__contact-text">{user.email}</Text>
                
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
                    <Text className="modern-template__contact-text">{user.phoneNumber}</Text>
                  </>
                )}
              </div>
              
              {user.bio && (
                <Paragraph 
                  className="modern-template__bio"
                  style={{
                    borderLeft: `3px solid ${sectionPrimaryColor}30`
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