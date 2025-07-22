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
    <Card key="personal" style={{ 
      marginBottom: '32px',
      position: 'relative',
      background: '#fff',
      borderRadius: '20px',
      boxShadow: '0 20px 40px rgba(0,0,0,0.1)',
      overflow: 'hidden'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      {/* Professional Header */}
      <div style={{
        background: `linear-gradient(135deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}dd)`,
        height: '120px',
        position: 'relative'
      }}>
        <div style={{
          position: 'absolute',
          bottom: '-40px',
          left: '40px'
        }}>
          <Avatar
            size={80}
            src={user.avatar ? `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${user.avatar}` : undefined}
            style={{ 
              border: '4px solid white',
              boxShadow: '0 8px 20px rgba(0,0,0,0.15)'
            }}
          />
        </div>
      </div>
      
      <div style={{ padding: '50px 40px 40px' }}>
        <Title level={2} style={{ 
          color: '#2c3e50', 
          marginBottom: '8px',
          fontWeight: '600'
        }}>
          {user.name}
        </Title>
        
        <Text style={{ 
          fontSize: '16px', 
          color: sectionPrimaryColor,
          fontWeight: '500',
          display: 'block',
          marginBottom: '20px'
        }}>
          {user.title || 'Professional'}
        </Text>
        
        <Row gutter={[24, 16]}>
          <Col xs={24} sm={12}>
            <div style={{ 
              display: 'flex', 
              alignItems: 'center', 
              marginBottom: '12px' 
            }}>
              <span style={{ 
                fontSize: '16px', 
                marginRight: '12px',
                color: sectionPrimaryColor
              }}>📧</span>
              <Text>{user.email}</Text>
            </div>
          </Col>
          {user.phoneNumber && (
            <Col xs={24} sm={12}>
              <div style={{ 
                display: 'flex', 
                alignItems: 'center', 
                marginBottom: '12px' 
              }}>
                <span style={{ 
                  fontSize: '16px', 
                  marginRight: '12px',
                  color: sectionPrimaryColor
                }}>📞</span>
                <Text>{user.phoneNumber}</Text>
              </div>
            </Col>
          )}
        </Row>
        
        {user.bio && (
          <div style={{ 
            marginTop: '24px',
            padding: '20px',
            background: '#f8f9fa',
            borderRadius: '12px',
            borderLeft: `4px solid ${sectionPrimaryColor}`
          }}>
            <Paragraph style={{ 
              fontSize: '15px', 
              lineHeight: 1.7,
              color: '#555',
              margin: 0
            }}>
              {user.bio}
            </Paragraph>
          </div>
        )}
      </div>
    </Card>
  );
};

export default ProfessionalTemplate; 