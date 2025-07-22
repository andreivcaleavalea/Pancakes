import React from 'react';
import { Card, Avatar, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './ProfessionalTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface ProfessionalTemplateProps {
  user: any;
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const ProfessionalTemplate: React.FC<ProfessionalTemplateProps> = ({
  user,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="personal" className="professional-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
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
            src={user.avatar ? `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${user.avatar}` : undefined}
            className="professional-template__avatar"
          />
        </div>
      </div>
      
      <div className="professional-template__content">
        <Title level={2} className="professional-template__name">
          {user.name}
        </Title>
        
        <Text 
          className="professional-template__title"
          style={{ color: sectionPrimaryColor }}
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
              <Text>{user.email}</Text>
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
                <Text>{user.phoneNumber}</Text>
              </div>
            </Col>
          )}
        </Row>
        
        {user.bio && (
          <div 
            className="professional-template__bio"
            style={{ borderLeft: `4px solid ${sectionPrimaryColor}` }}
          >
            <Paragraph className="professional-template__bio-text">
              {user.bio}
            </Paragraph>
          </div>
        )}
      </div>
    </Card>
  );
};

export default ProfessionalTemplate; 