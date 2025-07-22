import React from 'react';
import { Card, Avatar, Typography, Row, Col, Space } from 'antd';
import SectionSettingsPopover from '../SectionSettingsPopover';
import type { SectionRendererProps } from '../../types';
import { SECTION_COLORS } from '../../constants';

const { Title, Text, Paragraph } = Typography;

interface PersonalSectionProps extends Omit<SectionRendererProps, 'data'> {
  user: any;
}

const PersonalSection: React.FC<PersonalSectionProps> = ({
  sectionKey,
  user,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  const { template } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

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

  const renderHeroTemplate = () => (
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

  const renderProfessionalTemplate = () => (
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

  const renderCreativeTemplate = () => (
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

  const renderModernTemplate = () => (
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

  const renderCardTemplate = () => (
    <Card key="personal" style={{ 
      marginBottom: '24px', 
      textAlign: 'center', 
      padding: '32px',
      position: 'relative',
      borderRadius: '12px'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Avatar
        size={100}
        src={user.avatar ? `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${user.avatar}` : undefined}
        style={{ marginBottom: '20px', border: `3px solid ${sectionPrimaryColor}` }}
      />
      <Title level={2} style={{ color: sectionPrimaryColor, margin: '20px 0 8px' }}>
        {user.name}
      </Title>
      <Text type="secondary" style={{ fontSize: '16px', display: 'block', marginBottom: '12px' }}>
        {user.email}
      </Text>
      {user.phoneNumber && (
        <Text type="secondary" style={{ fontSize: '14px', display: 'block', marginBottom: '16px' }}>
          📞 {user.phoneNumber}
        </Text>
      )}
      {user.bio && (
        <Paragraph style={{ marginTop: '20px', fontSize: '14px' }}>
          {user.bio}
        </Paragraph>
      )}
    </Card>
  );

  const renderMinimalTemplate = () => (
    <Card key="personal" style={{ 
      marginBottom: '24px', 
      textAlign: 'center', 
      padding: '32px', 
      position: 'relative' 
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Avatar
        size={80}
        src={user.avatar ? `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${user.avatar}` : undefined}
        style={{ marginBottom: '16px', border: `3px solid ${sectionPrimaryColor}` }}
      />
      <Title level={2} style={{ color: sectionPrimaryColor, margin: '16px 0 8px' }}>
        {user.name}
      </Title>
      <Text type="secondary" style={{ fontSize: '16px' }}>{user.email}</Text>
      {user.bio && (
        <Paragraph style={{ marginTop: '16px', fontSize: '14px' }}>
          {user.bio}
        </Paragraph>
      )}
    </Card>
  );

  // Template selector
  switch (template) {
    case 'hero':
      return renderHeroTemplate();
    case 'professional':
      return renderProfessionalTemplate();
    case 'creative':
      return renderCreativeTemplate();
    case 'modern':
      return renderModernTemplate();
    case 'card':
      return renderCardTemplate();
    case 'minimal':
      return renderMinimalTemplate();
    default:
      return renderCardTemplate(); // fallback
  }
};

export default PersonalSection; 