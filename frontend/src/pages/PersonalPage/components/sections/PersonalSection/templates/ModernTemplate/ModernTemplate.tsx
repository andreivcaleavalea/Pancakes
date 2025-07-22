import React from 'react';
import { Card, Avatar, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
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
    <Card key="personal" style={{ 
      marginBottom: '32px',
      position: 'relative',
      background: '#fff',
      borderRadius: '16px',
      border: 'none',
      boxShadow: '0 8px 40px rgba(0,0,0,0.12)',
      overflow: 'hidden'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      {/* Modern geometric pattern */}
      <div style={{
        position: 'absolute',
        top: 0,
        left: 0,
        right: 0,
        height: '6px',
        background: `linear-gradient(90deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80, ${sectionPrimaryColor}60)`
      }} />
      
      <div style={{ padding: '32px' }}>
        <Row gutter={[32, 24]}>
          <Col xs={24} sm={6}>
            <div style={{ textAlign: 'center' }}>
              <div style={{
                position: 'relative',
                display: 'inline-block'
              }}>
                <Avatar
                  size={90}
                  src={user.avatar ? `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${user.avatar}` : undefined}
                  style={{ 
                    border: `3px solid ${sectionPrimaryColor}20`
                  }}
                />
                <div style={{
                  position: 'absolute',
                  bottom: '5px',
                  right: '5px',
                  width: '20px',
                  height: '20px',
                  background: '#52c41a',
                  borderRadius: '50%',
                  border: '3px solid white'
                }} />
              </div>
            </div>
          </Col>
          <Col xs={24} sm={18}>
            <div>
              <Title level={2} style={{ 
                color: '#1a1a1a', 
                marginBottom: '8px',
                fontWeight: '600',
                fontSize: '24px'
              }}>
                {user.name}
              </Title>
              
              <Text style={{ 
                fontSize: '14px', 
                color: '#999',
                textTransform: 'uppercase',
                letterSpacing: '1px',
                display: 'block',
                marginBottom: '16px'
              }}>
                {user.title || 'Professional Profile'}
              </Text>
              
              <div style={{
                display: 'grid',
                gridTemplateColumns: 'auto 1fr',
                gap: '12px 16px',
                marginBottom: '20px'
              }}>
                <div style={{
                  width: '32px',
                  height: '32px',
                  background: `${sectionPrimaryColor}15`,
                  borderRadius: '8px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center'
                }}>
                  <span style={{ fontSize: '14px' }}>📧</span>
                </div>
                <Text style={{ fontSize: '15px', color: '#555' }}>{user.email}</Text>
                
                {user.phoneNumber && (
                  <>
                    <div style={{
                      width: '32px',
                      height: '32px',
                      background: `${sectionPrimaryColor}15`,
                      borderRadius: '8px',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center'
                    }}>
                      <span style={{ fontSize: '14px' }}>📞</span>
                    </div>
                    <Text style={{ fontSize: '15px', color: '#555' }}>{user.phoneNumber}</Text>
                  </>
                )}
              </div>
              
              {user.bio && (
                <Paragraph style={{ 
                  fontSize: '15px', 
                  lineHeight: 1.6,
                  color: '#666',
                  margin: 0,
                  paddingLeft: '16px',
                  borderLeft: `3px solid ${sectionPrimaryColor}30`
                }}>
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