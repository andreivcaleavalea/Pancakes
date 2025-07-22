import React from 'react';
import { Card, Avatar, Typography, Row, Col, Space } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
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
    <Card key="personal" style={{ 
      marginBottom: '32px',
      position: 'relative',
      background: `linear-gradient(135deg, ${sectionPrimaryColor}10, ${sectionPrimaryColor}05)`,
      border: `1px solid ${sectionPrimaryColor}20`,
      borderRadius: '16px'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <div style={{ padding: '40px 20px' }}>
        <Row align="middle" gutter={[32, 24]}>
          <Col xs={24} sm={8} md={6}>
            <div style={{ textAlign: 'center' }}>
              <div style={{
                background: `linear-gradient(45deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}90)`,
                borderRadius: '20px',
                padding: '16px',
                display: 'inline-block',
                marginBottom: '16px'
              }}>
                <Avatar
                  size={120}
                  src={user.avatar ? `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${user.avatar}` : undefined}
                  style={{ border: '4px solid white' }}
                />
              </div>
            </div>
          </Col>
          <Col xs={24} sm={16} md={18}>
            <Space direction="vertical" size="large" style={{ width: '100%' }}>
              <div>
                <Title level={1} style={{ 
                  color: '#2c3e50', 
                  marginBottom: '8px',
                  fontWeight: '700',
                  letterSpacing: '-1px'
                }}>
                  {user.name}
                </Title>
                <div style={{
                  background: `${sectionPrimaryColor}15`,
                  padding: '12px 20px',
                  borderRadius: '12px',
                  marginBottom: '20px',
                  borderLeft: `4px solid ${sectionPrimaryColor}`
                }}>
                  <Text style={{ fontSize: '18px', color: '#555' }}>
                    📧 {user.email}
                    {user.phoneNumber && <span style={{ marginLeft: '20px' }}>📞 {user.phoneNumber}</span>}
                  </Text>
                </div>
              </div>
              {user.bio && (
                <Paragraph style={{ 
                  fontSize: '16px', 
                  lineHeight: 1.8,
                  color: '#666',
                  margin: 0
                }}>
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