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
  // Add CSS animation keyframes for creative template
  React.useEffect(() => {
    const styleSheet = document.createElement('style');
    styleSheet.type = 'text/css';
    styleSheet.innerHTML = `
      @keyframes rotate {
        from { transform: rotate(0deg); }
        to { transform: rotate(360deg); }
      }
      @keyframes pulse {
        0% { transform: scale(1); }
        50% { transform: scale(1.05); }
        100% { transform: scale(1); }
      }
    `;
    document.head.appendChild(styleSheet);
    
    return () => {
      document.head.removeChild(styleSheet);
    };
  }, []);

  return (
    <Card key="personal" style={{ 
      marginBottom: '32px',
      position: 'relative',
      borderRadius: '24px',
      overflow: 'hidden',
      background: `linear-gradient(45deg, ${sectionPrimaryColor}05, ${sectionPrimaryColor}15)`,
      border: 'none',
      boxShadow: '0 15px 35px rgba(0,0,0,0.1)'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      {/* Creative Background Pattern */}
      <div style={{
        position: 'absolute',
        top: 0,
        right: 0,
        width: '200px',
        height: '200px',
        background: `radial-gradient(circle, ${sectionPrimaryColor}20, transparent)`,
        borderRadius: '50%',
        transform: 'translate(50px, -50px)'
      }} />
      
      <div style={{ padding: '40px', position: 'relative', zIndex: 1 }}>
        <Row align="middle" gutter={[32, 24]}>
          <Col xs={24} md={8}>
            <div style={{ textAlign: 'center' }}>
              <div style={{
                position: 'relative',
                display: 'inline-block',
                marginBottom: '20px'
              }}>
                {/* Rotating Ring Animation */}
                <div style={{
                  position: 'absolute',
                  top: '-15px',
                  left: '-15px',
                  right: '-15px',
                  bottom: '-15px',
                  background: `conic-gradient(${sectionPrimaryColor}60, transparent, ${sectionPrimaryColor}60, transparent, ${sectionPrimaryColor}60)`,
                  borderRadius: '50%',
                  animation: 'rotate 8s linear infinite',
                  zIndex: 1
                }} />
                
                {/* Pulsing Inner Ring */}
                <div style={{
                  position: 'absolute',
                  top: '-8px',
                  left: '-8px',
                  right: '-8px',
                  bottom: '-8px',
                  background: `linear-gradient(45deg, ${sectionPrimaryColor}30, transparent, ${sectionPrimaryColor}30)`,
                  borderRadius: '50%',
                  animation: 'pulse 3s ease-in-out infinite',
                  zIndex: 1
                }} />
                
                <Avatar
                  size={100}
                  src={user.avatar ? `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${user.avatar}` : undefined}
                  style={{ 
                    border: '3px solid white',
                    position: 'relative',
                    zIndex: 2,
                    boxShadow: '0 8px 25px rgba(0,0,0,0.15)',
                    animation: 'rotate 6s linear infinite'
                  }}
                />
              </div>
            </div>
          </Col>
          <Col xs={24} md={16}>
            <div>
              <Title level={2} style={{ 
                color: '#2c3e50', 
                marginBottom: '12px',
                fontWeight: '700',
                fontSize: '28px'
              }}>
                {user.name}
              </Title>
              
              <div style={{
                display: 'flex',
                flexWrap: 'wrap',
                gap: '12px',
                marginBottom: '20px'
              }}>
                <div style={{
                  background: `${sectionPrimaryColor}15`,
                  padding: '8px 16px',
                  borderRadius: '20px',
                  border: `2px solid ${sectionPrimaryColor}30`
                }}>
                  <Text style={{ color: sectionPrimaryColor, fontWeight: '500' }}>
                    📧 {user.email}
                  </Text>
                </div>
                {user.phoneNumber && (
                  <div style={{
                    background: `${sectionPrimaryColor}15`,
                    padding: '8px 16px',
                    borderRadius: '20px',
                    border: `2px solid ${sectionPrimaryColor}30`
                  }}>
                    <Text style={{ color: sectionPrimaryColor, fontWeight: '500' }}>
                      📞 {user.phoneNumber}
                    </Text>
                  </div>
                )}
              </div>
              
              {user.bio && (
                <Paragraph style={{ 
                  fontSize: '16px', 
                  lineHeight: 1.8,
                  color: '#666',
                  margin: 0,
                  fontStyle: 'italic'
                }}>
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