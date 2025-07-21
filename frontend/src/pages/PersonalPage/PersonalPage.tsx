import React, { useState } from 'react';
import { Card, Button, Space, Typography, Switch, Spin, Alert, Avatar, Row, Col, Timeline, Tag, Form, Input, Select, App, Dropdown, Popover } from 'antd';
import { SettingOutlined, ShareAltOutlined, ArrowLeftOutlined } from '@ant-design/icons';
import { useRouter } from '../../router/RouterProvider';
import { useProfile } from '../../hooks/useProfile';
import { usePersonalPage } from '../../hooks/usePersonalPage';
import './PersonalPage.scss';

const { Title, Text, Paragraph } = Typography;
const { Option } = Select;
// Beautiful PersonalPageView component with real data and dynamic settings
const PersonalPageView: React.FC<{ 
  settings: any;
  sectionSettings: Record<string, any>;
  onSectionSettingsChange: (sectionKey: string, newSettings: any) => void;
}> = ({ settings, sectionSettings, onSectionSettingsChange }) => {
  const { profileData } = useProfile();
  
  if (!profileData) {
    return (
      <div style={{ padding: '24px' }}>
        <Card>
          <div style={{ textAlign: 'center', padding: '40px' }}>
            <Spin size="large" />
            <Title level={4} style={{ marginTop: '16px' }}>Loading your profile...</Title>
          </div>
        </Card>
      </div>
    );
  }

  const { user, educations, jobs, hobbies, projects } = profileData;
  
  // Get settings or use defaults
  const sectionOrder = settings?.sectionOrder || ['personal', 'education', 'jobs', 'projects', 'hobbies'];
  const sectionVisibility = settings?.sectionVisibility || {
    personal: true,
    education: true,
    jobs: true,
    projects: true,
    hobbies: true
  };
  const sectionTemplates = settings?.sectionTemplates || {
    personal: 'card',
    education: 'timeline',
    jobs: 'timeline',
    projects: 'grid',
    hobbies: 'tags'
  };
  const colorScheme = settings?.colorScheme || 'blue';
  
  // Color scheme mapping
  const colors = {
    blue: '#1890ff',
    purple: '#722ed1',
    green: '#52c41a',
    orange: '#fa8c16',
    red: '#f5222d',
    gold: '#faad14'
  };
  
  const primaryColor = colors[colorScheme as keyof typeof colors] || colors.blue;

  // Template options for each section
  const allTemplateOptions = {
    personal: [
      { value: 'hero', label: '🦸 Hero Banner' },
      { value: 'minimal', label: '⭕ Minimal' },
      { value: 'creative', label: '🎨 Creative' },
      { value: 'professional', label: '💼 Executive' }
    ],
    education: [
      { value: 'academic', label: '🎓 Academic Timeline' },
      { value: 'university', label: '🏛️ University Cards' },
      { value: 'progress', label: '📈 Progress Journey' },
      { value: 'certificates', label: '🏆 Achievement Gallery' },
      { value: 'simple', label: '📋 Simple List' }
    ],
    jobs: [
      { value: 'career', label: '🚀 Career Journey' },
      { value: 'corporate', label: '🏢 Corporate Timeline' },
      { value: 'experience', label: '💼 Experience Cards' },
      { value: 'skills', label: '⚡ Skills Evolution' },
      { value: 'compact', label: '📊 Compact View' }
    ],
    projects: [
      { value: 'portfolio', label: '🎯 Portfolio Showcase' },
      { value: 'github', label: '💻 GitHub Style' },
      { value: 'masonry', label: '🧱 Masonry Grid' },
      { value: 'slider', label: '🎠 Project Slider' },
      { value: 'detailed', label: '📋 Detailed List' }
    ],
    hobbies: [
      { value: 'creative', label: '🎨 Creative Cloud' },
      { value: 'interactive', label: '⚡ Interactive Skills' },
      { value: 'badges', label: '🏆 Achievement Badges' },
      { value: 'radar', label: '📊 Skill Radar' },
      { value: 'minimal', label: '🏷️ Tag Cloud' }
    ]
  };

  // Section renderers
  const renderPersonalSection = () => {
    if (!sectionVisibility.personal) return null;
    
    const sectionKey = 'personal';
    const currentSectionSettings = sectionSettings[sectionKey] || { template: 'hero', color: 'blue' };
    const template = currentSectionSettings.template;
    const sectionColors = {
      blue: '#1890ff',
      purple: '#722ed1',
      green: '#52c41a',
      orange: '#fa8c16',
      red: '#f5222d',
      gold: '#faad14'
    };
    const sectionPrimaryColor = sectionColors[currentSectionSettings.color as keyof typeof sectionColors] || sectionColors.blue;
    
    const templateOptions = allTemplateOptions[sectionKey];
    
    if (template === 'hero') {
      return (
        <div key="personal" style={{
          background: `linear-gradient(135deg, ${sectionPrimaryColor}15, ${sectionPrimaryColor}05)`,
          borderRadius: '20px',
          padding: '40px',
          marginBottom: '32px',
          position: 'relative',
          overflow: 'hidden',
          border: `2px solid ${sectionPrimaryColor}20`
        }}>
          {/* Section Settings Icon */}
          <SectionSettingsPopover
            sectionKey={sectionKey}
            sectionSettings={currentSectionSettings}
            onSettingsChange={onSectionSettingsChange}
            templateOptions={templateOptions}
          />
          
          {/* Background Pattern */}
          <div style={{
            position: 'absolute',
            top: 0,
            right: 0,
            width: '200px',
            height: '200px',
            background: `radial-gradient(circle, ${sectionPrimaryColor}10, transparent)`,
            borderRadius: '50%',
            transform: 'translate(50%, -50%)'
          }} />
          
          <Row align="middle" gutter={[32, 32]}>
            <Col xs={24} md={8}>
              <div style={{ textAlign: 'center', position: 'relative' }}>
                <div style={{
                  background: `linear-gradient(45deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`,
                  borderRadius: '50%',
                  padding: '8px',
                  display: 'inline-block',
                  marginBottom: '16px',
                  boxShadow: `0 8px 32px ${sectionPrimaryColor}40`,
                  animation: 'pulse 2s infinite'
                }}>
                  <Avatar
                    size={140}
                    src={user.avatar ? `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${user.avatar}` : undefined}
                    style={{ border: '4px solid white' }}
                  />
                </div>
              </div>
            </Col>
            <Col xs={24} md={16}>
              <Title level={1} style={{ 
                fontSize: '48px', 
                background: `linear-gradient(45deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`,
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                marginBottom: '8px',
                fontWeight: 'bold'
              }}>
                {user.name}
              </Title>
              <div style={{ 
                background: 'rgba(255,255,255,0.9)', 
                padding: '16px 24px', 
                borderRadius: '12px',
                backdropFilter: 'blur(10px)',
                border: '1px solid rgba(255,255,255,0.2)',
                marginBottom: '20px'
              }}>
                <Text style={{ fontSize: '18px', color: '#666' }}>✉️ {user.email}</Text>
                {user.phoneNumber && (
                  <Text style={{ fontSize: '18px', color: '#666', marginLeft: '20px' }}>📞 {user.phoneNumber}</Text>
                )}
              </div>
              {user.bio && (
                <Paragraph style={{ 
                  fontSize: '18px', 
                  lineHeight: 1.8,
                  background: 'rgba(255,255,255,0.7)',
                  padding: '20px',
                  borderRadius: '12px',
                  border: `2px solid ${sectionPrimaryColor}20`
                }}>
                  {user.bio}
                </Paragraph>
              )}
            </Col>
          </Row>
          
          <style>{`
            @keyframes pulse {
              0%, 100% { transform: scale(1); }
              50% { transform: scale(1.05); }
            }
          `}</style>
        </div>
      );
    }
    
    if (template === 'creative') {
      return (
        <div key="personal" style={{
          background: `conic-gradient(from 0deg, ${primaryColor}20, transparent, ${primaryColor}10, transparent, ${primaryColor}20)`,
          borderRadius: '24px',
          padding: '32px',
          marginBottom: '32px',
          position: 'relative',
          overflow: 'hidden'
        }}>
          <div style={{
            background: 'rgba(255,255,255,0.95)',
            borderRadius: '20px',
            padding: '32px',
            backdropFilter: 'blur(20px)',
            border: '1px solid rgba(255,255,255,0.3)'
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '24px', flexWrap: 'wrap' }}>
              <div style={{ position: 'relative' }}>
                <div style={{
                  width: '120px',
                  height: '120px',
                  background: `conic-gradient(${primaryColor}, ${primaryColor}60, ${primaryColor})`,
                  borderRadius: '50%',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  animation: 'rotate 10s linear infinite'
                }}>
                  <Avatar
                    size={100}
                    src={user.avatar ? `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${user.avatar}` : undefined}
                    style={{ border: '3px solid white' }}
                  />
                </div>
              </div>
              <div style={{ flex: 1 }}>
                <Title level={2} style={{ 
                  color: primaryColor, 
                  marginBottom: '12px',
                  textShadow: `2px 2px 4px ${primaryColor}20`
                }}>
                  {user.name}
                </Title>
                <div style={{ 
                  display: 'flex', 
                  gap: '16px', 
                  flexWrap: 'wrap',
                  marginBottom: '16px'
                }}>
                  <Tag color={primaryColor} style={{ padding: '4px 12px', fontSize: '14px' }}>
                    ✉️ {user.email}
                  </Tag>
                  {user.phoneNumber && (
                    <Tag color={primaryColor} style={{ padding: '4px 12px', fontSize: '14px' }}>
                      📞 {user.phoneNumber}
                    </Tag>
                  )}
                </div>
                {user.bio && (
                  <Paragraph style={{ fontSize: '16px', lineHeight: 1.6, margin: 0 }}>
                    {user.bio}
                  </Paragraph>
                )}
              </div>
            </div>
          </div>
          
          <style>{`
            @keyframes rotate {
              0% { transform: rotate(0deg); }
              100% { transform: rotate(360deg); }
            }
          `}</style>
        </div>
      );
    }
    
    if (template === 'professional') {
      return (
        <Card key="personal" style={{
          marginBottom: '32px',
          background: `linear-gradient(145deg, #ffffff, #f8f9fa)`,
          border: `2px solid ${primaryColor}15`,
          borderRadius: '16px',
          boxShadow: '0 10px 40px rgba(0,0,0,0.1)'
        }}>
          <div style={{ padding: '20px' }}>
            <Row align="middle" gutter={[32, 24]}>
              <Col xs={24} sm={6}>
                <div style={{ textAlign: 'center' }}>
                  <div style={{
                    background: `linear-gradient(45deg, ${primaryColor}, ${primaryColor}90)`,
                    borderRadius: '20px',
                    padding: '16px',
                                         display: 'inline-block',
                     marginBottom: '16px'
                   }}>
                     <Avatar
                      size={100}
                      src={user.avatar ? `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${user.avatar}` : undefined}
                      style={{ border: '3px solid white' }}
                    />
                  </div>
                </div>
              </Col>
              <Col xs={24} sm={18}>
                <Title level={2} style={{ 
                  color: '#2c3e50', 
                  marginBottom: '8px',
                  fontWeight: '700',
                  letterSpacing: '-0.5px'
                }}>
                  {user.name}
                </Title>
                <div style={{
                  background: `${primaryColor}10`,
                  padding: '12px 16px',
                                     borderRadius: '8px',
                   marginBottom: '16px',
                   borderLeft: `4px solid ${sectionPrimaryColor}`
                }}>
                  <Text style={{ fontSize: '16px', color: '#555' }}>
                    📧 {user.email}
                    {user.phoneNumber && <span style={{ marginLeft: '20px' }}>📞 {user.phoneNumber}</span>}
                  </Text>
                </div>
                {user.bio && (
                  <Paragraph style={{ 
                    fontSize: '16px', 
                    lineHeight: 1.7,
                    color: '#666',
                    margin: 0
                  }}>
                    {user.bio}
                  </Paragraph>
                )}
              </Col>
            </Row>
          </div>
        </Card>
      );
    }
    
    // Minimal template (fallback)
    return (
      <Card key="personal" style={{ marginBottom: '24px', textAlign: 'center', padding: '32px', position: 'relative' }}>
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
  };

  const renderEducationSection = () => {
    if (!sectionVisibility.education || !educations || educations.length === 0) return null;
    
    const sectionKey = 'education';
    const currentSectionSettings = sectionSettings[sectionKey] || { template: 'academic', color: 'blue' };
    const template = currentSectionSettings.template;
    const sectionColors = {
      blue: '#1890ff',
      purple: '#722ed1',
      green: '#52c41a',
      orange: '#fa8c16',
      red: '#f5222d',
      gold: '#faad14'
    };
    const sectionPrimaryColor = sectionColors[currentSectionSettings.color as keyof typeof sectionColors] || sectionColors.blue;
    const templateOptions = allTemplateOptions[sectionKey];
    
          if (template === 'academic') {
        return (
          <Card key="education" style={{ 
            marginBottom: '32px',
            background: `linear-gradient(135deg, ${sectionPrimaryColor}08, ${sectionPrimaryColor}03)`,
            border: `2px solid ${sectionPrimaryColor}20`,
            borderRadius: '16px',
            position: 'relative'
          }}>
            {/* Section Settings Icon */}
            <SectionSettingsPopover
              sectionKey={sectionKey}
              sectionSettings={currentSectionSettings}
              onSettingsChange={onSectionSettingsChange}
              templateOptions={templateOptions}
            />
            
            <Title level={2} style={{ 
              color: sectionPrimaryColor, 
              marginBottom: '24px',
              textAlign: 'center',
              fontSize: '28px'
            }}>🎓 Academic Journey</Title>
          
          <div style={{ position: 'relative' }}>
            {/* Academic Timeline with Progress */}
            <div style={{
              position: 'absolute',
              left: '50%',
              top: 0,
              bottom: 0,
              width: '4px',
              background: `linear-gradient(to bottom, ${primaryColor}, ${primaryColor}60)`,
              transform: 'translateX(-50%)',
              borderRadius: '2px'
            }} />
            
            {educations.map((edu: any, index: number) => (
              <div key={index} style={{ 
                display: 'flex', 
                alignItems: 'center',
                marginBottom: index < educations.length - 1 ? '40px' : '0',
                position: 'relative'
              }}>
                {/* Year Badge */}
                <div style={{
                  position: 'absolute',
                  left: '50%',
                  transform: 'translateX(-50%)',
                  background: sectionPrimaryColor,
                  color: 'white',
                  padding: '8px 16px',
                  borderRadius: '20px',
                  fontSize: '12px',
                  fontWeight: 'bold',
                  zIndex: 2,
                  boxShadow: `0 4px 12px ${sectionPrimaryColor}40`
                }}>
                  {edu.startDate?.split('-')[0]} - {edu.endDate?.split('-')[0] || 'Now'}
                </div>
                
                {/* Left side (odd) / Right side (even) */}
                <div style={{
                  width: index % 2 === 0 ? '45%' : '100%',
                  marginLeft: index % 2 === 0 ? 0 : '55%',
                  marginRight: index % 2 === 0 ? '55%' : 0
                }}>
                  <div style={{
                    background: 'white',
                    padding: '24px',
                    borderRadius: '12px',
                    boxShadow: `0 8px 25px ${sectionPrimaryColor}20`,
                    border: `1px solid ${sectionPrimaryColor}20`,
                    position: 'relative',
                    transform: 'translateY(20px)'
                  }}>
                    {/* Arrow pointing to timeline */}
                    <div style={{
                      position: 'absolute',
                      top: '24px',
                      [index % 2 === 0 ? 'right' : 'left']: '-10px',
                      width: 0,
                      height: 0,
                      borderTop: '10px solid transparent',
                      borderBottom: '10px solid transparent',
                      [index % 2 === 0 ? 'borderLeft' : 'borderRight']: `10px solid white`
                    }} />
                    
                    <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '12px' }}>
                      <div style={{
                        background: `linear-gradient(45deg, ${primaryColor}, ${primaryColor}80)`,
                        borderRadius: '50%',
                        width: '40px',
                        height: '40px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: '18px'
                      }}>
                        🏛️
                      </div>
                      <Title level={4} style={{ margin: 0, color: primaryColor }}>
                        {edu.institution}
                      </Title>
                    </div>
                    
                    <Text strong style={{ fontSize: '16px', color: '#333' }}>
                      {edu.degree} in {edu.specialization}
                    </Text>
                    
                    {edu.description && (
                      <Paragraph style={{ 
                        marginTop: '12px', 
                        color: '#666',
                        lineHeight: 1.6,
                        fontSize: '14px'
                      }}>
                        {edu.description}
                      </Paragraph>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </Card>
      );
    }
    
    if (template === 'university') {
      return (
        <Card key="education" style={{ marginBottom: '32px', borderRadius: '16px' }}>
          <Title level={2} style={{ color: primaryColor, textAlign: 'center', marginBottom: '32px' }}>
            🏛️ University Experience
          </Title>
          
          <Row gutter={[24, 24]}>
            {educations.map((edu: any, index: number) => (
              <Col xs={24} md={12} lg={8} key={index}>
                <div style={{
                  background: `linear-gradient(145deg, white, ${primaryColor}05)`,
                  border: `2px solid ${primaryColor}20`,
                  borderRadius: '20px',
                  padding: '24px',
                  height: '100%',
                  position: 'relative',
                  overflow: 'hidden',
                  transition: 'all 0.3s ease',
                  cursor: 'pointer'
                }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.transform = 'translateY(-8px)';
                  e.currentTarget.style.boxShadow = `0 20px 40px ${primaryColor}30`;
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.transform = 'translateY(0)';
                  e.currentTarget.style.boxShadow = 'none';
                }}>
                  {/* University Badge */}
                  <div style={{
                    position: 'absolute',
                    top: '-10px',
                    right: '20px',
                    background: `linear-gradient(45deg, ${primaryColor}, ${primaryColor}80)`,
                    color: 'white',
                    padding: '8px 16px',
                    borderRadius: '20px',
                    fontSize: '12px',
                    fontWeight: 'bold',
                    boxShadow: `0 4px 12px ${primaryColor}40`
                  }}>
                    🎓 GRADUATE
                  </div>
                  
                  {/* Institution Header */}
                  <div style={{ textAlign: 'center', marginBottom: '20px' }}>
                    <div style={{
                      background: `conic-gradient(${primaryColor}, ${primaryColor}60, ${primaryColor})`,
                      borderRadius: '50%',
                      width: '80px',
                      height: '80px',
                      margin: '0 auto 16px',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '32px',
                      animation: 'rotate 8s linear infinite'
                    }}>
                      🏛️
                    </div>
                    <Title level={4} style={{ 
                      margin: 0, 
                      color: primaryColor,
                      fontSize: '18px',
                      textAlign: 'center'
                    }}>
                      {edu.institution}
                    </Title>
                  </div>
                  
                  {/* Degree Info */}
                  <div style={{
                    background: 'rgba(255,255,255,0.8)',
                    padding: '16px',
                                         borderRadius: '12px',
                     marginBottom: '16px',
                     border: `1px solid ${primaryColor}15`
                  }}>
                    <Text strong style={{ fontSize: '15px', color: '#333' }}>
                      {edu.degree}
                    </Text>
                    <br />
                    <Text style={{ fontSize: '14px', color: primaryColor }}>
                      Specialization: {edu.specialization}
                    </Text>
                  </div>
                  
                  {/* Duration */}
                  <div style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    marginBottom: '12px'
                  }}>
                    <Tag color={primaryColor}>{edu.startDate}</Tag>
                    <span style={{ color: '#999' }}>→</span>
                    <Tag color={primaryColor}>{edu.endDate || 'Present'}</Tag>
                  </div>
                  
                  {edu.description && (
                    <Paragraph style={{ 
                      fontSize: '13px', 
                      color: '#666',
                      lineHeight: 1.5,
                      margin: 0
                    }}>
                      {edu.description}
                    </Paragraph>
                  )}
                </div>
              </Col>
            ))}
          </Row>
        </Card>
      );
    }
    
    if (template === 'progress') {
      return (
        <Card key="education" style={{ marginBottom: '32px', borderRadius: '16px' }}>
          <Title level={2} style={{ color: primaryColor, marginBottom: '32px' }}>
            📈 Learning Progress Journey
          </Title>
          
          {educations.map((edu: any, index: number) => (
            <div key={index} style={{
              background: `linear-gradient(90deg, ${primaryColor}05, ${primaryColor}02)`,
              borderRadius: '16px',
              padding: '24px',
              marginBottom: '24px',
              border: `1px solid ${primaryColor}20`,
              position: 'relative',
              overflow: 'hidden'
            }}>
              {/* Progress Bar */}
              <div style={{
                position: 'absolute',
                top: 0,
                left: 0,
                height: '6px',
                width: edu.endDate ? '100%' : '80%',
                background: `linear-gradient(90deg, ${primaryColor}, ${primaryColor}60)`,
                borderRadius: '3px'
              }} />
              
              <Row gutter={[24, 16]} align="middle">
                <Col xs={24} sm={4}>
                  <div style={{ textAlign: 'center' }}>
                    <div style={{
                      background: `linear-gradient(45deg, ${primaryColor}, ${primaryColor}80)`,
                      borderRadius: '50%',
                      width: '60px',
                      height: '60px',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      margin: '0 auto',
                      color: 'white',
                      fontSize: '24px',
                      boxShadow: `0 4px 15px ${primaryColor}40`
                    }}>
                      📚
                    </div>
                  </div>
                </Col>
                
                <Col xs={24} sm={14}>
                  <Title level={4} style={{ margin: '0 0 8px', color: primaryColor }}>
                    {edu.institution}
                  </Title>
                  <Text strong style={{ fontSize: '16px' }}>
                    {edu.degree} in {edu.specialization}
                  </Text>
                  {edu.description && (
                    <Paragraph style={{ marginTop: '8px', color: '#666', fontSize: '14px' }}>
                      {edu.description}
                    </Paragraph>
                  )}
                </Col>
                
                <Col xs={24} sm={6}>
                  <div style={{ textAlign: 'center' }}>
                    <div style={{
                      background: primaryColor,
                      color: 'white',
                      padding: '12px',
                      borderRadius: '12px',
                      fontSize: '14px',
                      fontWeight: 'bold'
                    }}>
                      {edu.startDate} - {edu.endDate || 'Present'}
                    </div>
                    {!edu.endDate && (
                      <Text style={{ fontSize: '12px', color: primaryColor, marginTop: '4px' }}>
                        🎯 In Progress
                      </Text>
                    )}
                  </div>
                </Col>
              </Row>
            </div>
          ))}
        </Card>
      );
    }
    
    if (template === 'certificates') {
      return (
        <Card key="education" style={{ marginBottom: '32px', borderRadius: '16px' }}>
          <Title level={2} style={{ color: primaryColor, textAlign: 'center', marginBottom: '32px' }}>
            🏆 Achievement Gallery
          </Title>
          
          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))',
            gap: '24px'
          }}>
            {educations.map((edu: any, index: number) => (
              <div key={index} style={{
                background: `linear-gradient(135deg, ${primaryColor}15, ${primaryColor}05)`,
                borderRadius: '20px',
                padding: '0',
                overflow: 'hidden',
                boxShadow: `0 10px 30px ${primaryColor}20`,
                border: `2px solid ${primaryColor}30`,
                position: 'relative'
              }}>
                {/* Certificate Header */}
                <div style={{
                  background: `linear-gradient(45deg, ${primaryColor}, ${primaryColor}80)`,
                  color: 'white',
                  padding: '20px',
                  textAlign: 'center',
                  position: 'relative'
                }}>
                  <div style={{
                    fontSize: '40px',
                    marginBottom: '8px'
                  }}>🏆</div>
                  <Title level={4} style={{ color: 'white', margin: 0 }}>
                    CERTIFICATE OF ACHIEVEMENT
                  </Title>
                </div>
                
                {/* Certificate Body */}
                <div style={{ padding: '24px', textAlign: 'center' }}>
                  <Text style={{ fontSize: '14px', color: '#666', marginBottom: '12px' }}>
                    This certifies that
                  </Text>
                  
                  <Title level={3} style={{ 
                    margin: '8px 0 16px', 
                    color: primaryColor,
                    fontStyle: 'italic'
                  }}>
                    {/* Assuming user name, could be dynamic */}
                    [Student Name]
                  </Title>
                  
                  <Text style={{ fontSize: '14px', color: '#666' }}>
                    has successfully completed
                  </Text>
                  
                  <div style={{
                    background: 'white',
                    padding: '16px',
                    borderRadius: '12px',
                    margin: '16px 0',
                    border: `2px solid ${primaryColor}20`
                  }}>
                    <Title level={4} style={{ margin: '0 0 8px', color: primaryColor }}>
                      {edu.degree}
                    </Title>
                    <Text strong>in {edu.specialization}</Text>
                    <br />
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      at {edu.institution}
                    </Text>
                  </div>
                  
                  <div style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    marginTop: '20px',
                    paddingTop: '16px',
                    borderTop: `1px solid ${primaryColor}20`
                  }}>
                    <div>
                      <Text style={{ fontSize: '12px', color: '#999' }}>Start Date</Text>
                      <br />
                      <Text strong>{edu.startDate}</Text>
                    </div>
                    <div>
                      <Text style={{ fontSize: '12px', color: '#999' }}>Completion</Text>
                      <br />
                      <Text strong>{edu.endDate || 'In Progress'}</Text>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </Card>
      );
    }
    
    // Simple list (fallback)
    return (
      <Card key="education" style={{ marginBottom: '24px', position: 'relative' }}>
        {/* Section Settings Icon */}
        <SectionSettingsPopover
          sectionKey={sectionKey}
          sectionSettings={currentSectionSettings}
          onSettingsChange={onSectionSettingsChange}
          templateOptions={templateOptions}
        />
        
        <Title level={2} style={{ color: sectionPrimaryColor }}>🎓 Education</Title>
        {educations.map((edu: any, index: number) => (
          <div key={index} style={{ 
            padding: '16px 0', 
            borderBottom: index < educations.length - 1 ? `1px solid ${primaryColor}20` : 'none' 
          }}>
            <Text strong style={{ fontSize: '16px' }}>{edu.institution}</Text>
            <br />
            <Text>{edu.degree} in {edu.specialization}</Text>
            <br />
            <Text type="secondary">{edu.startDate} - {edu.endDate || 'Present'}</Text>
            {edu.description && (
              <Paragraph style={{ marginTop: '8px', color: '#666' }}>
                {edu.description}
              </Paragraph>
            )}
          </div>
        ))}
      </Card>
    );
  };

  const renderJobsSection = () => {
    if (!sectionVisibility.jobs || !jobs || jobs.length === 0) return null;
    
    const sectionKey = 'jobs';
    const currentSectionSettings = sectionSettings[sectionKey] || { template: 'career', color: 'blue' };
    const template = currentSectionSettings.template;
    const sectionColors = {
      blue: '#1890ff',
      purple: '#722ed1',
      green: '#52c41a',
      orange: '#fa8c16',
      red: '#f5222d',
      gold: '#faad14'
    };
    const sectionPrimaryColor = sectionColors[currentSectionSettings.color as keyof typeof sectionColors] || sectionColors.blue;
    const templateOptions = allTemplateOptions[sectionKey];
    
    if (template === 'career') {
      return (
        <Card key="jobs" style={{ 
          marginBottom: '32px',
          position: 'relative'
        }}>
          {/* Section Settings Icon */}
          <SectionSettingsPopover
            sectionKey={sectionKey}
            sectionSettings={currentSectionSettings}
            onSettingsChange={onSectionSettingsChange}
            templateOptions={templateOptions}
          />
          
          <Title level={2} style={{ color: sectionPrimaryColor, marginBottom: '32px' }}>
            🚀 Career Journey
          </Title>
          
          <div style={{ padding: '20px' }}>
            {jobs.map((job: any, index: number) => (
              <div key={index} style={{
                background: '#fafafa',
                borderRadius: '12px',
                padding: '24px',
                marginBottom: '20px',
                border: `1px solid ${sectionPrimaryColor}20`,
                position: 'relative'
              }}>
                <Row align="middle" gutter={[32, 24]}>
                  <Col xs={24} sm={6}>
                    <div style={{ textAlign: 'center' }}>
                      <div style={{
                        background: `linear-gradient(45deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}90)`,
                        borderRadius: '50%',
                        width: '80px',
                        height: '80px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        margin: '0 auto 16px',
                        fontSize: '24px'
                      }}>
                        💼
                      </div>
                      <Tag color={sectionPrimaryColor}>
                        {job.startDate} - {job.endDate || 'Present'}
                      </Tag>
                    </div>
                  </Col>
                  <Col xs={24} sm={18}>
                    <Title level={3} style={{ 
                      color: '#2c3e50', 
                      marginBottom: '8px',
                      fontWeight: '700'
                    }}>
                      {job.position}
                    </Title>
                    <Title level={4} style={{ 
                      color: sectionPrimaryColor, 
                      marginBottom: '12px',
                      fontWeight: '600'
                    }}>
                      {job.company}
                    </Title>
                    {job.location && (
                      <Text style={{ fontSize: '14px', color: '#666', marginBottom: '12px', display: 'block' }}>
                        📍 {job.location}
                      </Text>
                    )}
                    {job.description && (
                      <Paragraph style={{ 
                        fontSize: '15px', 
                        lineHeight: 1.6,
                        color: '#555',
                        margin: 0
                      }}>
                        {job.description}
                      </Paragraph>
                    )}
                  </Col>
                </Row>
              </div>
            ))}
          </div>
        </Card>
      );
    }
    
    if (template === 'corporate') {
      return (
        <Card key="jobs" style={{ marginBottom: '32px', borderRadius: '16px' }}>
          <Title level={2} style={{ color: primaryColor, textAlign: 'center', marginBottom: '32px' }}>
            🏢 Corporate Timeline
          </Title>
          
          <Row gutter={[24, 24]}>
            {jobs.map((job: any, index: number) => (
              <Col xs={24} md={12} lg={8} key={index}>
                <div style={{
                  background: `linear-gradient(145deg, white, ${primaryColor}05)`,
                  border: `2px solid ${primaryColor}20`,
                  borderRadius: '20px',
                  padding: '24px',
                  height: '100%',
                  position: 'relative',
                  overflow: 'hidden',
                  transition: 'all 0.3s ease',
                  cursor: 'pointer'
                }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.transform = 'translateY(-8px)';
                  e.currentTarget.style.boxShadow = `0 20px 40px ${primaryColor}30`;
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.transform = 'translateY(0)';
                  e.currentTarget.style.boxShadow = 'none';
                }}>
                  {/* Corporate Badge */}
                  <div style={{
                    position: 'absolute',
                    top: '-10px',
                    right: '20px',
                    background: `linear-gradient(45deg, ${primaryColor}, ${primaryColor}80)`,
                    color: 'white',
                    padding: '8px 16px',
                    borderRadius: '20px',
                    fontSize: '12px',
                    fontWeight: 'bold',
                    boxShadow: `0 4px 12px ${primaryColor}40`
                  }}>
                    🏢 CORPORATE
                  </div>
                  
                  {/* Job Header */}
                  <div style={{ textAlign: 'center', marginBottom: '20px' }}>
                    <div style={{
                      background: `conic-gradient(${primaryColor}, ${primaryColor}60, ${primaryColor})`,
                      borderRadius: '50%',
                      width: '80px',
                      height: '80px',
                      margin: '0 auto 16px',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '32px',
                      animation: 'rotate 8s linear infinite'
                    }}>
                      🏢
                    </div>
                    <Title level={4} style={{ 
                      margin: 0, 
                      color: primaryColor,
                      fontSize: '18px',
                      textAlign: 'center'
                    }}>
                      {job.company}
                    </Title>
                  </div>
                  
                  {/* Job Details */}
                  <div style={{
                    background: 'rgba(255,255,255,0.8)',
                    padding: '16px',
                                         borderRadius: '12px',
                     marginBottom: '16px',
                     border: `1px solid ${primaryColor}15`
                  }}>
                    <Text strong style={{ fontSize: '15px', color: '#333' }}>
                      {job.position}
                    </Text>
                    <br />
                    <Text style={{ fontSize: '14px', color: primaryColor }}>
                      {job.location}
                    </Text>
                  </div>
                  
                  {/* Duration */}
                  <div style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    marginBottom: '12px'
                  }}>
                    <Tag color={primaryColor}>{job.startDate}</Tag>
                    <span style={{ color: '#999' }}>→</span>
                    <Tag color={primaryColor}>{job.endDate || 'Present'}</Tag>
                  </div>
                  
                  {job.description && (
                    <Paragraph style={{ 
                      fontSize: '13px', 
                      color: '#666',
                      lineHeight: 1.5,
                      margin: 0
                    }}>
                      {job.description}
                    </Paragraph>
                  )}
                </div>
              </Col>
            ))}
          </Row>
        </Card>
      );
    }
    
    if (template === 'experience') {
      return (
        <Card key="jobs" style={{ marginBottom: '32px', borderRadius: '16px' }}>
          <Title level={2} style={{ color: primaryColor, textAlign: 'center', marginBottom: '32px' }}>
            💼 Experience Cards
          </Title>
          
          <Row gutter={[24, 24]}>
            {jobs.map((job: any, index: number) => (
              <Col xs={24} md={12} lg={8} key={index}>
                <div style={{
                  background: `linear-gradient(145deg, white, ${primaryColor}05)`,
                  border: `2px solid ${primaryColor}20`,
                  borderRadius: '20px',
                  padding: '24px',
                  height: '100%',
                  position: 'relative',
                  overflow: 'hidden',
                  transition: 'all 0.3s ease',
                  cursor: 'pointer'
                }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.transform = 'translateY(-8px)';
                  e.currentTarget.style.boxShadow = `0 20px 40px ${primaryColor}30`;
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.transform = 'translateY(0)';
                  e.currentTarget.style.boxShadow = 'none';
                }}>
                  {/* Experience Badge */}
                  <div style={{
                    position: 'absolute',
                    top: '-10px',
                    right: '20px',
                    background: `linear-gradient(45deg, ${primaryColor}, ${primaryColor}80)`,
                    color: 'white',
                    padding: '8px 16px',
                    borderRadius: '20px',
                    fontSize: '12px',
                    fontWeight: 'bold',
                    boxShadow: `0 4px 12px ${primaryColor}40`
                  }}>
                    💼 EXPERIENCE
                  </div>
                  
                  {/* Job Header */}
                  <div style={{ textAlign: 'center', marginBottom: '20px' }}>
                    <div style={{
                      background: `conic-gradient(${primaryColor}, ${primaryColor}60, ${primaryColor})`,
                      borderRadius: '50%',
                      width: '80px',
                      height: '80px',
                      margin: '0 auto 16px',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '32px',
                      animation: 'rotate 8s linear infinite'
                    }}>
                      💼
                    </div>
                    <Title level={4} style={{ 
                      margin: 0, 
                      color: primaryColor,
                      fontSize: '18px',
                      textAlign: 'center'
                    }}>
                      {job.company}
                    </Title>
                  </div>
                  
                  {/* Job Details */}
                  <div style={{
                    background: 'rgba(255,255,255,0.8)',
                    padding: '16px',
                                         borderRadius: '12px',
                     marginBottom: '16px',
                     border: `1px solid ${primaryColor}15`
                  }}>
                    <Text strong style={{ fontSize: '15px', color: '#333' }}>
                      {job.position}
                    </Text>
                    <br />
                    <Text style={{ fontSize: '14px', color: primaryColor }}>
                      {job.location}
                    </Text>
                  </div>
                  
                  {/* Duration */}
                  <div style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    marginBottom: '12px'
                  }}>
                    <Tag color={primaryColor}>{job.startDate}</Tag>
                    <span style={{ color: '#999' }}>→</span>
                    <Tag color={primaryColor}>{job.endDate || 'Present'}</Tag>
                  </div>
                  
                  {job.description && (
                    <Paragraph style={{ 
                      fontSize: '13px', 
                      color: '#666',
                      lineHeight: 1.5,
                      margin: 0
                    }}>
                      {job.description}
                    </Paragraph>
                  )}
                </div>
              </Col>
            ))}
          </Row>
        </Card>
      );
    }
    
    if (template === 'skills') {
      return (
        <Card key="jobs" style={{ marginBottom: '32px', borderRadius: '16px' }}>
          <Title level={2} style={{ color: primaryColor, textAlign: 'center', marginBottom: '32px' }}>
            ⚡ Skills Evolution
          </Title>
          
          <Row gutter={[24, 24]}>
            {jobs.map((job: any, index: number) => (
              <Col xs={24} md={12} lg={8} key={index}>
                <div style={{
                  background: `linear-gradient(145deg, white, ${primaryColor}05)`,
                  border: `2px solid ${primaryColor}20`,
                  borderRadius: '20px',
                  padding: '24px',
                  height: '100%',
                  position: 'relative',
                  overflow: 'hidden',
                  transition: 'all 0.3s ease',
                  cursor: 'pointer'
                }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.transform = 'translateY(-8px)';
                  e.currentTarget.style.boxShadow = `0 20px 40px ${primaryColor}30`;
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.transform = 'translateY(0)';
                  e.currentTarget.style.boxShadow = 'none';
                }}>
                  {/* Skills Badge */}
                  <div style={{
                    position: 'absolute',
                    top: '-10px',
                    right: '20px',
                    background: `linear-gradient(45deg, ${primaryColor}, ${primaryColor}80)`,
                    color: 'white',
                    padding: '8px 16px',
                    borderRadius: '20px',
                    fontSize: '12px',
                    fontWeight: 'bold',
                    boxShadow: `0 4px 12px ${primaryColor}40`
                  }}>
                    ⚡ SKILLS
                  </div>
                  
                  {/* Job Header */}
                  <div style={{ textAlign: 'center', marginBottom: '20px' }}>
                    <div style={{
                      background: `conic-gradient(${primaryColor}, ${primaryColor}60, ${primaryColor})`,
                      borderRadius: '50%',
                      width: '80px',
                      height: '80px',
                      margin: '0 auto 16px',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '32px',
                      animation: 'rotate 8s linear infinite'
                    }}>
                      ⚡
                    </div>
                    <Title level={4} style={{ 
                      margin: 0, 
                      color: primaryColor,
                      fontSize: '18px',
                      textAlign: 'center'
                    }}>
                      {job.company}
                    </Title>
                  </div>
                  
                  {/* Skill Details */}
                  <div style={{
                    background: 'rgba(255,255,255,0.8)',
                    padding: '16px',
                                         borderRadius: '12px',
                     marginBottom: '16px',
                     border: `1px solid ${primaryColor}15`
                  }}>
                    {job.skills.map((skill: any, index: number) => (
                      <div key={index} style={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center',
                        marginBottom: '12px'
                      }}>
                        <Text style={{ fontSize: '14px', color: '#666' }}>
                          {skill.name}
                        </Text>
                        <Tag color={primaryColor} style={{ fontSize: '10px' }}>
                          {skill.level}
                        </Tag>
                      </div>
                    ))}
                  </div>
                  
                  {/* Duration */}
                  <div style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    marginBottom: '12px'
                  }}>
                    <Tag color={primaryColor}>{job.startDate}</Tag>
                    <span style={{ color: '#999' }}>→</span>
                    <Tag color={primaryColor}>{job.endDate || 'Present'}</Tag>
                  </div>
                  
                  {job.description && (
                    <Paragraph style={{ 
                      fontSize: '13px', 
                      color: '#666',
                      lineHeight: 1.5,
                      margin: 0
                    }}>
                      {job.description}
                    </Paragraph>
                  )}
                </div>
              </Col>
            ))}
          </Row>
        </Card>
      );
    }
    
    if (template === 'compact') {
      return (
        <Card key="jobs" style={{ marginBottom: '24px', textAlign: 'center', padding: '32px' }}>
          <Title level={2} style={{ color: primaryColor, marginBottom: '24px' }}>
            📊 Compact View
          </Title>
          
          <div style={{ 
            display: 'flex', 
            flexWrap: 'wrap', 
            gap: '12px',
            justifyContent: 'center'
          }}>
            {jobs.map((job: any, index: number) => (
              <div key={index} style={{
                background: `linear-gradient(45deg, ${primaryColor}, ${primaryColor}80)`,
                color: 'white',
                padding: '12px 20px',
                borderRadius: '25px',
                fontSize: '14px',
                fontWeight: 'bold',
                boxShadow: `0 4px 15px ${primaryColor}30`,
                transition: 'all 0.3s ease',
                cursor: 'pointer'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.transform = 'scale(1.1)';
                e.currentTarget.style.boxShadow = `0 6px 20px ${primaryColor}50`;
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.transform = 'scale(1)';
                e.currentTarget.style.boxShadow = `0 4px 15px ${primaryColor}30`;
              }}>
                {job.position} at {job.company}
              </div>
            ))}
          </div>
        </Card>
      );
    }
    
         // Timeline (default)
     return (
       <Card key="jobs" style={{ marginBottom: '24px', position: 'relative' }}>
         {/* Section Settings Icon */}
         <SectionSettingsPopover
           sectionKey={sectionKey}
           sectionSettings={currentSectionSettings}
           onSettingsChange={onSectionSettingsChange}
           templateOptions={templateOptions}
         />
         
         <Title level={2} style={{ color: sectionPrimaryColor }}>💼 Work Experience</Title>
        
        {jobs.map((job: any, index: number) => (
          <div key={index} style={{ padding: '12px 0', borderBottom: index < jobs.length - 1 ? '1px solid #f0f0f0' : 'none' }}>
            <Text strong>{job.position}</Text> at <Text>{job.company}</Text>
            <br />
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {job.startDate} - {job.endDate || 'Present'}
            </Text>
          </div>
        ))}
      </Card>
    );
  };

  const renderProjectsSection = () => {
    if (!sectionVisibility.projects || !projects || projects.length === 0) return null;
    
    const sectionKey = 'projects';
    const currentSectionSettings = sectionSettings[sectionKey] || { template: 'portfolio', color: 'blue' };
    const template = currentSectionSettings.template;
    const sectionColors = {
      blue: '#1890ff',
      purple: '#722ed1',
      green: '#52c41a',
      orange: '#fa8c16',
      red: '#f5222d',
      gold: '#faad14'
    };
    const sectionPrimaryColor = sectionColors[currentSectionSettings.color as keyof typeof sectionColors] || sectionColors.blue;
    const templateOptions = allTemplateOptions[sectionKey];
    
    if (template === 'portfolio') {
      return (
        <Card key="projects" style={{ 
          marginBottom: '32px',
          borderRadius: '20px',
          overflow: 'hidden',
          background: `linear-gradient(135deg, ${sectionPrimaryColor}08, ${sectionPrimaryColor}03)`,
          position: 'relative'
        }}>
          {/* Section Settings Icon */}
          <SectionSettingsPopover
            sectionKey={sectionKey}
            sectionSettings={currentSectionSettings}
            onSettingsChange={onSectionSettingsChange}
            templateOptions={templateOptions}
          />
          
          <div style={{
            background: `linear-gradient(45deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`,
            color: 'white',
            padding: '32px',
            textAlign: 'center',
            marginBottom: '32px'
          }}>
            <Title level={2} style={{ color: 'white', margin: '0 0 8px', fontSize: '32px' }}>
              🎯 Portfolio Showcase
            </Title>
            <Text style={{ color: 'rgba(255,255,255,0.9)', fontSize: '16px' }}>
              Crafted with passion & precision
            </Text>
          </div>
          
          <div style={{ padding: '0 32px 32px' }}>
            <Row gutter={[24, 24]}>
              {projects.map((project: any, index: number) => (
                <Col xs={24} md={12} lg={8} key={index}>
                  <div style={{
                    background: 'white',
                    borderRadius: '16px',
                    overflow: 'hidden',
                    boxShadow: '0 8px 30px rgba(0,0,0,0.1)',
                    border: `2px solid ${primaryColor}15`,
                    transition: 'all 0.4s ease',
                    cursor: 'pointer'
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.transform = 'translateY(-12px) scale(1.02)';
                    e.currentTarget.style.boxShadow = `0 20px 50px ${primaryColor}30`;
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.transform = 'translateY(0) scale(1)';
                    e.currentTarget.style.boxShadow = '0 8px 30px rgba(0,0,0,0.1)';
                  }}>
                    {/* Project Header */}
                    <div style={{
                      background: `linear-gradient(135deg, ${primaryColor}15, ${primaryColor}05)`,
                      padding: '24px',
                      textAlign: 'center',
                      position: 'relative'
                    }}>
                      <div style={{
                        background: `conic-gradient(${primaryColor}, ${primaryColor}60, ${primaryColor})`,
                        borderRadius: '50%',
                        width: '60px',
                        height: '60px',
                        margin: '0 auto 16px',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: '24px',
                        animation: 'rotate 6s linear infinite'
                      }}>
                        🚀
                      </div>
                      <Title level={4} style={{ margin: 0, color: primaryColor }}>
                        {project.name}
                      </Title>
                    </div>
                    
                    {/* Project Content */}
                    <div style={{ padding: '20px' }}>
                      <Paragraph style={{ 
                        color: '#666', 
                        lineHeight: 1.6,
                        marginBottom: '16px',
                        fontSize: '14px'
                      }}>
                        {project.description}
                      </Paragraph>
                      
                      {/* Tech Stack */}
                      {project.technologies && (
                        <div style={{ marginBottom: '20px' }}>
                          <div style={{ 
                            display: 'flex', 
                            flexWrap: 'wrap', 
                            gap: '6px',
                            justifyContent: 'center'
                          }}>
                            {project.technologies.split(',').slice(0, 4).map((tech: string, techIndex: number) => (
                              <div key={techIndex} style={{
                                background: `${primaryColor}15`,
                                color: primaryColor,
                                padding: '4px 8px',
                                borderRadius: '12px',
                                fontSize: '11px',
                                fontWeight: 'bold',
                                border: `1px solid ${primaryColor}30`
                              }}>
                                {tech.trim()}
                              </div>
                            ))}
                          </div>
                        </div>
                      )}
                      
                      {/* Action Buttons */}
                      <div style={{ 
                        display: 'flex', 
                        gap: '8px',
                        justifyContent: 'center'
                      }}>
                        {project.projectUrl && (
                          <a href={project.projectUrl} target="_blank" rel="noopener noreferrer">
                            <Button type="primary" size="small" style={{
                              background: primaryColor,
                              borderColor: primaryColor,
                              borderRadius: '20px'
                            }}>
                              🌐 Live Demo
                            </Button>
                          </a>
                        )}
                        {project.githubUrl && (
                          <a href={project.githubUrl} target="_blank" rel="noopener noreferrer">
                            <Button size="small" style={{
                              borderColor: primaryColor,
                              color: primaryColor,
                              borderRadius: '20px'
                            }}>
                              💻 Code
                            </Button>
                          </a>
                        )}
                      </div>
                    </div>
                  </div>
                </Col>
              ))}
            </Row>
          </div>
        </Card>
      );
    }
    
    if (template === 'github') {
      return (
        <Card key="projects" style={{ 
          marginBottom: '32px',
          borderRadius: '12px',
          background: '#0d1117',
          border: '1px solid #30363d'
        }}>
          <div style={{ padding: '24px' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '24px' }}>
              <div style={{
                background: '#21262d',
                borderRadius: '50%',
                width: '40px',
                height: '40px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                fontSize: '20px'
              }}>
                💻
              </div>
              <Title level={2} style={{ color: '#f0f6fc', margin: 0 }}>
                Repositories
              </Title>
            </div>
            
            <div style={{ display: 'grid', gap: '16px' }}>
              {projects.map((project: any, index: number) => (
                <div key={index} style={{
                  background: '#161b22',
                  border: '1px solid #30363d',
                  borderRadius: '8px',
                  padding: '20px',
                  transition: 'all 0.2s ease'
                }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.borderColor = primaryColor;
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.borderColor = '#30363d';
                }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', marginBottom: '12px' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                      <div style={{ color: '#7d8590', fontSize: '16px' }}>📁</div>
                      <Title level={4} style={{ color: primaryColor, margin: 0 }}>
                        {project.name}
                      </Title>
                      <div style={{
                        background: '#21262d',
                        border: '1px solid #30363d',
                        borderRadius: '12px',
                        padding: '2px 6px',
                        fontSize: '10px',
                        color: '#7d8590'
                      }}>
                        Public
                      </div>
                    </div>
                    
                    <div style={{ display: 'flex', gap: '8px' }}>
                      {project.projectUrl && (
                        <a href={project.projectUrl} target="_blank" rel="noopener noreferrer">
                          <Button size="small" style={{
                            background: 'transparent',
                            borderColor: '#30363d',
                            color: '#f0f6fc',
                            fontSize: '12px'
                          }}>
                            🌐
                          </Button>
                        </a>
                      )}
                      {project.githubUrl && (
                        <a href={project.githubUrl} target="_blank" rel="noopener noreferrer">
                          <Button size="small" style={{
                            background: 'transparent',
                            borderColor: '#30363d',
                            color: '#f0f6fc',
                            fontSize: '12px'
                          }}>
                            ⭐
                          </Button>
                        </a>
                      )}
                    </div>
                  </div>
                  
                  <Paragraph style={{ 
                    color: '#8b949e', 
                    fontSize: '14px',
                    lineHeight: 1.5,
                    marginBottom: '16px'
                  }}>
                    {project.description}
                  </Paragraph>
                  
                  <div style={{ display: 'flex', alignItems: 'center', gap: '16px', fontSize: '12px' }}>
                    {project.technologies && (
                      <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                        <div style={{
                          width: '12px',
                          height: '12px',
                          borderRadius: '50%',
                          background: primaryColor
                        }} />
                        <span style={{ color: '#f0f6fc' }}>
                          {project.technologies.split(',')[0]?.trim()}
                        </span>
                      </div>
                    )}
                    <span style={{ color: '#8b949e' }}>Updated recently</span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </Card>
      );
    }
    
    if (template === 'masonry') {
      return (
        <Card key="projects" style={{ marginBottom: '32px', borderRadius: '16px' }}>
          <Title level={2} style={{ color: primaryColor, textAlign: 'center', marginBottom: '32px' }}>
            🧱 Project Gallery
          </Title>
          
          <div style={{
            columns: '350px',
            columnGap: '24px',
            columnFill: 'balance'
          }}>
            {projects.map((project: any, index: number) => (
              <div key={index} style={{
                background: `linear-gradient(135deg, ${primaryColor}08, white)`,
                borderRadius: '16px',
                padding: '20px',
                marginBottom: '24px',
                breakInside: 'avoid',
                border: `2px solid ${primaryColor}20`,
                boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
                transition: 'all 0.3s ease'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.transform = 'scale(1.02)';
                e.currentTarget.style.boxShadow = `0 8px 30px ${primaryColor}25`;
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.transform = 'scale(1)';
                e.currentTarget.style.boxShadow = '0 4px 20px rgba(0,0,0,0.08)';
              }}>
                <div style={{ 
                  display: 'flex', 
                  alignItems: 'center', 
                  gap: '12px',
                  marginBottom: '16px'
                }}>
                  <div style={{
                    background: primaryColor,
                    borderRadius: '8px',
                    width: '32px',
                    height: '32px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '16px'
                  }}>
                    🎯
                  </div>
                  <Title level={4} style={{ margin: 0, color: primaryColor }}>
                    {project.name}
                  </Title>
                </div>
                
                <Paragraph style={{ 
                  color: '#666', 
                  lineHeight: 1.6,
                  marginBottom: '16px'
                }}>
                  {project.description}
                </Paragraph>
                
                {project.technologies && (
                  <div style={{ marginBottom: '16px' }}>
                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px' }}>
                      {project.technologies.split(',').map((tech: string, techIndex: number) => (
                        <span key={techIndex} style={{
                          background: `${primaryColor}20`,
                          color: primaryColor,
                          padding: '2px 8px',
                          borderRadius: '10px',
                          fontSize: '11px',
                          fontWeight: 'bold'
                        }}>
                          {tech.trim()}
                        </span>
                      ))}
                    </div>
                  </div>
                )}
                
                <div style={{ display: 'flex', gap: '8px' }}>
                  {project.projectUrl && (
                    <a href={project.projectUrl} target="_blank" rel="noopener noreferrer">
                      <Button size="small" type="primary" style={{ 
                        background: primaryColor,
                        borderColor: primaryColor,
                        fontSize: '12px'
                      }}>
                        View
                      </Button>
                    </a>
                  )}
                  {project.githubUrl && (
                    <a href={project.githubUrl} target="_blank" rel="noopener noreferrer">
                      <Button size="small" style={{ 
                        borderColor: primaryColor,
                        color: primaryColor,
                        fontSize: '12px'
                      }}>
                        Code
                      </Button>
                    </a>
                  )}
                </div>
              </div>
            ))}
          </div>
        </Card>
      );
    }
    
    if (template === 'slider') {
      return (
        <Card key="projects" style={{ 
          marginBottom: '32px',
          borderRadius: '20px',
          overflow: 'hidden'
        }}>
          <Title level={2} style={{ color: primaryColor, textAlign: 'center', marginBottom: '32px' }}>
            🎠 Project Showcase Slider
          </Title>
          
          <div style={{ position: 'relative', padding: '0 60px' }}>
            <div style={{
              display: 'flex',
              gap: '24px',
              overflowX: 'auto',
              scrollBehavior: 'smooth',
              paddingBottom: '20px'
            }}>
              {projects.map((project: any, index: number) => (
                <div key={index} style={{
                  minWidth: '320px',
                  background: `linear-gradient(135deg, ${primaryColor}12, ${primaryColor}04)`,
                  borderRadius: '20px',
                  padding: '24px',
                  border: `2px solid ${primaryColor}25`,
                  position: 'relative',
                  overflow: 'hidden'
                }}>
                  {/* Slide Number */}
                  <div style={{
                    position: 'absolute',
                    top: '16px',
                    right: '16px',
                    background: primaryColor,
                    color: 'white',
                    width: '32px',
                    height: '32px',
                    borderRadius: '50%',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '14px',
                    fontWeight: 'bold'
                  }}>
                    {index + 1}
                  </div>
                  
                  {/* Project Hero */}
                  <div style={{
                    background: `linear-gradient(45deg, ${primaryColor}, ${primaryColor}80)`,
                    borderRadius: '16px',
                    padding: '32px',
                    textAlign: 'center',
                    marginBottom: '20px',
                    color: 'white'
                  }}>
                    <div style={{ fontSize: '32px', marginBottom: '12px' }}>🚀</div>
                    <Title level={3} style={{ color: 'white', margin: 0 }}>
                      {project.name}
                    </Title>
                  </div>
                  
                  <Paragraph style={{ 
                    color: '#666',
                    lineHeight: 1.6,
                    marginBottom: '20px',
                    textAlign: 'center'
                  }}>
                    {project.description}
                  </Paragraph>
                  
                  {project.technologies && (
                    <div style={{ 
                      display: 'flex', 
                      flexWrap: 'wrap', 
                      gap: '8px',
                      justifyContent: 'center',
                      marginBottom: '20px'
                    }}>
                      {project.technologies.split(',').slice(0, 3).map((tech: string, techIndex: number) => (
                        <Tag key={techIndex} color={primaryColor} style={{ margin: 0 }}>
                          {tech.trim()}
                        </Tag>
                      ))}
                    </div>
                  )}
                  
                  <div style={{ 
                    display: 'flex', 
                    gap: '12px',
                    justifyContent: 'center'
                  }}>
                    {project.projectUrl && (
                      <a href={project.projectUrl} target="_blank" rel="noopener noreferrer">
                        <Button type="primary" style={{
                          background: primaryColor,
                          borderColor: primaryColor,
                          borderRadius: '20px'
                        }}>
                          🌐 Demo
                        </Button>
                      </a>
                    )}
                    {project.githubUrl && (
                      <a href={project.githubUrl} target="_blank" rel="noopener noreferrer">
                        <Button style={{
                          borderColor: primaryColor,
                          color: primaryColor,
                          borderRadius: '20px'
                        }}>
                          💻 GitHub
                        </Button>
                      </a>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </Card>
      );
    }
    
    // Detailed list (fallback)
    return (
      <Card key="projects" style={{ marginBottom: '24px', position: 'relative' }}>
        {/* Section Settings Icon */}
        <SectionSettingsPopover
          sectionKey={sectionKey}
          sectionSettings={currentSectionSettings}
          onSettingsChange={onSectionSettingsChange}
          templateOptions={templateOptions}
        />
        
        <Title level={2} style={{ color: sectionPrimaryColor }}>🚀 Projects</Title>
        {projects.map((project: any, index: number) => (
          <div key={index} style={{ 
            padding: '20px 0', 
            borderBottom: index < projects.length - 1 ? `1px solid ${primaryColor}20` : 'none' 
          }}>
            <Title level={4} style={{ margin: '0 0 12px', color: primaryColor }}>
              {project.name}
            </Title>
            <Paragraph style={{ marginBottom: '12px' }}>
              {project.description}
            </Paragraph>
            {project.technologies && (
              <div style={{ marginBottom: '12px' }}>
                {project.technologies.split(',').map((tech: string, techIndex: number) => (
                  <Tag key={techIndex} color={primaryColor} style={{ margin: '2px' }}>
                    {tech.trim()}
                  </Tag>
                ))}
              </div>
            )}
            <Space>
              {project.projectUrl && (
                <a href={project.projectUrl} target="_blank" rel="noopener noreferrer">
                  <Button type="primary" size="small">View Project</Button>
                </a>
              )}
              {project.githubUrl && (
                <a href={project.githubUrl} target="_blank" rel="noopener noreferrer">
                  <Button size="small">GitHub</Button>
                </a>
              )}
            </Space>
          </div>
        ))}
      </Card>
    );
  };

  const renderHobbiesSection = () => {
    if (!sectionVisibility.hobbies || !hobbies || hobbies.length === 0) return null;
    
    const sectionKey = 'hobbies';
    const currentSectionSettings = sectionSettings[sectionKey] || { template: 'creative', color: 'blue' };
    const template = currentSectionSettings.template;
    const sectionColors = {
      blue: '#1890ff',
      purple: '#722ed1',
      green: '#52c41a',
      orange: '#fa8c16',
      red: '#f5222d',
      gold: '#faad14'
    };
    const sectionPrimaryColor = sectionColors[currentSectionSettings.color as keyof typeof sectionColors] || sectionColors.blue;
    const templateOptions = allTemplateOptions[sectionKey];
    
    if (template === 'creative') {
      return (
        <Card key="hobbies" style={{ 
          marginBottom: '32px',
          borderRadius: '20px',
          background: `radial-gradient(circle at top right, ${sectionPrimaryColor}12, ${sectionPrimaryColor}04, transparent)`,
          position: 'relative'
        }}>
          {/* Section Settings Icon */}
          <SectionSettingsPopover
            sectionKey={sectionKey}
            sectionSettings={currentSectionSettings}
            onSettingsChange={onSectionSettingsChange}
            templateOptions={templateOptions}
          />
          
          <Title level={2} style={{ 
            color: sectionPrimaryColor, 
            textAlign: 'center', 
            marginBottom: '32px',
            fontSize: '28px'
          }}>
            🎨 Creative Passions
          </Title>
          
          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))',
            gap: '20px',
            padding: '0 16px'
          }}>
            {hobbies.map((hobby: any, index: number) => (
              <div key={index} style={{
                background: `linear-gradient(135deg, ${sectionPrimaryColor}15, ${sectionPrimaryColor}05)`,
                borderRadius: '20px',
                padding: '24px',
                position: 'relative',
                overflow: 'hidden',
                border: `2px solid ${sectionPrimaryColor}25`,
                transition: 'all 0.4s ease',
                cursor: 'pointer'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.transform = 'translateY(-8px) rotate(2deg)';
                e.currentTarget.style.boxShadow = `0 20px 40px ${sectionPrimaryColor}30`;
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.transform = 'translateY(0) rotate(0deg)';
                e.currentTarget.style.boxShadow = 'none';
              }}>
                {/* Floating Elements */}
                <div style={{
                  position: 'absolute',
                  top: '10px',
                  right: '10px',
                  fontSize: '20px',
                  opacity: 0.3,
                  animation: 'float 3s ease-in-out infinite'
                }}>
                  ✨
                </div>
                
                {/* Hobby Icon & Name */}
                <div style={{ textAlign: 'center', marginBottom: '16px' }}>
                  <div style={{
                    background: `conic-gradient(${sectionPrimaryColor}, ${sectionPrimaryColor}60, ${sectionPrimaryColor})`,
                    borderRadius: '50%',
                    width: '70px',
                    height: '70px',
                    margin: '0 auto 16px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '28px',
                    animation: 'pulse 2s infinite'
                  }}>
                    🎯
                  </div>
                  
                  <Title level={4} style={{ 
                    margin: 0, 
                    color: sectionPrimaryColor,
                    textShadow: `1px 1px 2px ${sectionPrimaryColor}20`
                  }}>
                    {hobby.name}
                  </Title>
                </div>
                
                {/* Skill Level with Creative Design */}
                <div style={{
                  background: 'rgba(255,255,255,0.9)',
                  borderRadius: '15px',
                  padding: '12px',
                  marginBottom: '16px',
                  textAlign: 'center',
                  border: `1px solid ${sectionPrimaryColor}20`
                }}>
                  <Text style={{ fontSize: '12px', color: '#666', marginBottom: '8px' }}>
                    Skill Level
                  </Text>
                  <div style={{ position: 'relative' }}>
                    <div style={{
                      background: `${sectionPrimaryColor}20`,
                      borderRadius: '10px',
                      height: '20px',
                      overflow: 'hidden',
                      marginBottom: '8px'
                    }}>
                      <div style={{
                        background: `linear-gradient(90deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`,
                        height: '100%',
                        width: hobby.level === 'Expert' ? '100%' : hobby.level === 'Advanced' ? '75%' : hobby.level === 'Intermediate' ? '50%' : '25%',
                        borderRadius: '10px',
                        position: 'relative',
                        transition: 'width 0.8s ease'
                      }}>
                        {/* Sparkle effect */}
                        <div style={{
                          position: 'absolute',
                          right: '8px',
                          top: '50%',
                          transform: 'translateY(-50%)',
                          color: 'white',
                          fontSize: '12px',
                          fontWeight: 'bold'
                        }}>
                          ✨
                        </div>
                      </div>
                    </div>
                    <Text strong style={{ color: sectionPrimaryColor, fontSize: '14px' }}>
                      {hobby.level}
                    </Text>
                  </div>
                </div>
                
                {/* Description */}
                {hobby.description && (
                  <Paragraph style={{ 
                    fontSize: '13px', 
                    color: '#666',
                    lineHeight: 1.5,
                    textAlign: 'center',
                    margin: 0,
                    fontStyle: 'italic'
                  }}>
                    "{hobby.description}"
                  </Paragraph>
                )}
              </div>
            ))}
          </div>
          
          <style>{`
            @keyframes float {
              0%, 100% { transform: translateY(0px); }
              50% { transform: translateY(-10px); }
            }
          `}</style>
        </Card>
      );
    }
    
    if (template === 'interactive') {
      return (
        <Card key="hobbies" style={{ marginBottom: '32px', borderRadius: '16px' }}>
          <Title level={2} style={{ color: primaryColor, textAlign: 'center', marginBottom: '32px' }}>
            ⚡ Interactive Skills Dashboard
          </Title>
          
          <Row gutter={[24, 24]}>
            {hobbies.map((hobby: any, index: number) => (
              <Col xs={24} sm={12} md={8} key={index}>
                <div style={{
                  background: `linear-gradient(135deg, ${primaryColor}10, white)`,
                  borderRadius: '16px',
                  padding: '20px',
                  border: `2px solid ${primaryColor}20`,
                  height: '200px',
                  display: 'flex',
                  flexDirection: 'column',
                  justifyContent: 'center',
                  alignItems: 'center',
                  transition: 'all 0.3s ease',
                  cursor: 'pointer'
                }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.background = `linear-gradient(135deg, ${primaryColor}20, ${primaryColor}10)`;
                  e.currentTarget.style.transform = 'scale(1.05)';
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.background = `linear-gradient(135deg, ${primaryColor}10, white)`;
                  e.currentTarget.style.transform = 'scale(1)';
                }}>
                  {/* Circular Progress */}
                  <div style={{
                    position: 'relative',
                    width: '100px',
                    height: '100px',
                    marginBottom: '16px'
                  }}>
                    <svg width="100" height="100" style={{ transform: 'rotate(-90deg)' }}>
                      <circle
                        cx="50"
                        cy="50"
                        r="40"
                        stroke={`${primaryColor}20`}
                        strokeWidth="8"
                        fill="transparent"
                      />
                      <circle
                        cx="50"
                        cy="50"
                        r="40"
                        stroke={primaryColor}
                        strokeWidth="8"
                        fill="transparent"
                        strokeDasharray={`${2 * Math.PI * 40 * (
                          hobby.level === 'Expert' ? 0.9 : 
                          hobby.level === 'Advanced' ? 0.75 : 
                          hobby.level === 'Intermediate' ? 0.5 : 0.25
                        )} ${2 * Math.PI * 40}`}
                        strokeLinecap="round"
                        style={{ transition: 'stroke-dasharray 1s ease' }}
                      />
                    </svg>
                    
                    {/* Center Content */}
                    <div style={{
                      position: 'absolute',
                      top: '50%',
                      left: '50%',
                      transform: 'translate(-50%, -50%)',
                      textAlign: 'center'
                    }}>
                      <div style={{ fontSize: '24px', marginBottom: '4px' }}>🎯</div>
                      <Text style={{ fontSize: '12px', fontWeight: 'bold', color: primaryColor }}>
                        {hobby.level === 'Expert' ? '90%' : 
                         hobby.level === 'Advanced' ? '75%' : 
                         hobby.level === 'Intermediate' ? '50%' : '25%'}
                      </Text>
                    </div>
                  </div>
                  
                  <Title level={4} style={{ margin: '0 0 8px', color: primaryColor, textAlign: 'center' }}>
                    {hobby.name}
                  </Title>
                  
                  <Tag color={primaryColor} style={{ fontSize: '11px' }}>
                    {hobby.level}
                  </Tag>
                </div>
              </Col>
            ))}
          </Row>
        </Card>
      );
    }
    
    if (template === 'badges') {
      return (
        <Card key="hobbies" style={{ 
          marginBottom: '32px',
          background: `linear-gradient(135deg, ${primaryColor}08, ${primaryColor}03)`,
          borderRadius: '20px'
        }}>
          <Title level={2} style={{ color: primaryColor, textAlign: 'center', marginBottom: '32px' }}>
            🏆 Achievement Badges
          </Title>
          
          <div style={{
            display: 'flex',
            flexWrap: 'wrap',
            gap: '20px',
            justifyContent: 'center',
            padding: '20px'
          }}>
            {hobbies.map((hobby: any, index: number) => (
              <div key={index} style={{
                background: 'white',
                borderRadius: '20px',
                padding: '24px',
                textAlign: 'center',
                minWidth: '180px',
                border: `3px solid ${primaryColor}30`,
                boxShadow: `0 10px 30px ${primaryColor}20`,
                transition: 'all 0.4s ease',
                cursor: 'pointer'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.transform = 'translateY(-10px) scale(1.05)';
                e.currentTarget.style.boxShadow = `0 20px 50px ${primaryColor}40`;
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.transform = 'translateY(0) scale(1)';
                e.currentTarget.style.boxShadow = `0 10px 30px ${primaryColor}20`;
              }}>
                {/* Badge Design */}
                <div style={{
                  position: 'relative',
                  marginBottom: '16px'
                }}>
                  {/* Outer Ring */}
                  <div style={{
                    width: '80px',
                    height: '80px',
                    borderRadius: '50%',
                    background: `conic-gradient(${primaryColor}, ${primaryColor}60, ${primaryColor})`,
                    margin: '0 auto',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    animation: 'rotate 8s linear infinite'
                  }}>
                    {/* Inner Badge */}
                    <div style={{
                      width: '60px',
                      height: '60px',
                      borderRadius: '50%',
                      background: 'white',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '24px',
                      boxShadow: `inset 0 0 10px ${primaryColor}30`
                    }}>
                      🏆
                    </div>
                  </div>
                  
                  {/* Skill Level Ribbon */}
                  <div style={{
                    position: 'absolute',
                    bottom: '-8px',
                    left: '50%',
                    transform: 'translateX(-50%)',
                    background: hobby.level === 'Expert' ? '#ff6b35' : 
                               hobby.level === 'Advanced' ? '#4ecdc4' : 
                               hobby.level === 'Intermediate' ? '#45b7d1' : '#96ceb4',
                    color: 'white',
                    padding: '4px 12px',
                    borderRadius: '12px',
                    fontSize: '10px',
                    fontWeight: 'bold',
                    textTransform: 'uppercase',
                    letterSpacing: '0.5px'
                  }}>
                    {hobby.level}
                  </div>
                </div>
                
                <Title level={4} style={{ 
                  margin: '0 0 8px', 
                  color: primaryColor,
                  fontSize: '16px'
                }}>
                  {hobby.name}
                </Title>
                
                {hobby.description && (
                  <Text style={{ 
                    fontSize: '12px', 
                    color: '#666',
                    lineHeight: 1.4
                  }}>
                    {hobby.description}
                  </Text>
                )}
              </div>
            ))}
          </div>
        </Card>
      );
    }
    
    if (template === 'radar') {
      return (
        <Card key="hobbies" style={{ marginBottom: '32px', borderRadius: '16px' }}>
          <Title level={2} style={{ color: primaryColor, textAlign: 'center', marginBottom: '32px' }}>
            📊 Skills Radar Chart
          </Title>
          
          <Row gutter={[32, 32]} align="middle">
            <Col xs={24} lg={12}>
              {/* Radar Chart Visualization */}
              <div style={{
                position: 'relative',
                width: '300px',
                height: '300px',
                margin: '0 auto',
                background: `radial-gradient(circle, ${primaryColor}08, transparent)`
              }}>
                <svg width="300" height="300" style={{ position: 'absolute', top: 0, left: 0 }}>
                  {/* Grid Lines */}
                  {[60, 120, 180].map(radius => (
                    <circle
                      key={radius}
                      cx="150"
                      cy="150"
                      r={radius}
                      stroke={`${primaryColor}30`}
                      strokeWidth="1"
                      fill="none"
                    />
                  ))}
                  
                  {/* Axis Lines */}
                  {hobbies.slice(0, 6).map((_, index) => {
                    const angle = (index * 60) * (Math.PI / 180);
                    const x2 = 150 + 180 * Math.cos(angle);
                    const y2 = 150 + 180 * Math.sin(angle);
                    return (
                      <line
                        key={index}
                        x1="150"
                        y1="150"
                        x2={x2}
                        y2={y2}
                        stroke={`${primaryColor}20`}
                        strokeWidth="1"
                      />
                    );
                  })}
                  
                  {/* Data Points */}
                  {hobbies.slice(0, 6).map((hobby, index) => {
                    const skillValue = hobby.level === 'Expert' ? 180 : 
                                     hobby.level === 'Advanced' ? 135 : 
                                     hobby.level === 'Intermediate' ? 90 : 45;
                    const angle = (index * 60) * (Math.PI / 180);
                    const x = 150 + skillValue * Math.cos(angle);
                    const y = 150 + skillValue * Math.sin(angle);
                    
                    return (
                      <circle
                        key={index}
                        cx={x}
                        cy={y}
                        r="6"
                        fill={primaryColor}
                        stroke="white"
                        strokeWidth="2"
                      />
                    );
                  })}
                </svg>
                
                {/* Labels */}
                {hobbies.slice(0, 6).map((hobby, index) => {
                  const angle = (index * 60) * (Math.PI / 180);
                  const x = 150 + 220 * Math.cos(angle);
                  const y = 150 + 220 * Math.sin(angle);
                  
                  return (
                    <div
                      key={index}
                      style={{
                        position: 'absolute',
                        left: `${x - 40}px`,
                        top: `${y - 10}px`,
                        width: '80px',
                        textAlign: 'center',
                        fontSize: '12px',
                        fontWeight: 'bold',
                        color: primaryColor
                      }}
                    >
                      {hobby.name}
                    </div>
                  );
                })}
              </div>
            </Col>
            
            <Col xs={24} lg={12}>
              {/* Legend */}
              <div style={{ padding: '20px' }}>
                <Title level={4} style={{ color: primaryColor, marginBottom: '20px' }}>
                  Skills Overview
                </Title>
                
                {hobbies.map((hobby, index) => (
                  <div key={index} style={{
                    display: 'flex',
                    alignItems: 'center',
                    marginBottom: '16px',
                    padding: '12px',
                    background: `${primaryColor}08`,
                    borderRadius: '8px',
                    border: `1px solid ${primaryColor}20`
                  }}>
                    <div style={{
                      width: '12px',
                      height: '12px',
                      borderRadius: '50%',
                      background: primaryColor,
                      marginRight: '12px'
                    }} />
                    
                    <div style={{ flex: 1 }}>
                      <Text strong>{hobby.name}</Text>
                      <br />
                      <Tag color={primaryColor} style={{ fontSize: '10px' }}>
                        {hobby.level}
                      </Tag>
                    </div>
                  </div>
                ))}
              </div>
            </Col>
          </Row>
        </Card>
      );
    }
    
    // Minimal tag cloud (fallback)
    return (
      <Card key="hobbies" style={{ marginBottom: '24px', textAlign: 'center', padding: '32px', position: 'relative' }}>
        {/* Section Settings Icon */}
        <SectionSettingsPopover
          sectionKey={sectionKey}
          sectionSettings={currentSectionSettings}
          onSettingsChange={onSectionSettingsChange}
          templateOptions={templateOptions}
        />
        
        <Title level={2} style={{ color: sectionPrimaryColor, marginBottom: '24px' }}>
          🏷️ Interests & Hobbies
        </Title>
        
        <div style={{ 
          display: 'flex', 
          flexWrap: 'wrap', 
          gap: '12px',
          justifyContent: 'center'
        }}>
          {hobbies.map((hobby: any, index: number) => (
            <div key={index} style={{
              background: `linear-gradient(45deg, ${primaryColor}, ${primaryColor}80)`,
              color: 'white',
              padding: '12px 20px',
              borderRadius: '25px',
              fontSize: '14px',
              fontWeight: 'bold',
              boxShadow: `0 4px 15px ${primaryColor}30`,
              transition: 'all 0.3s ease',
              cursor: 'pointer'
            }}
            onMouseEnter={(e) => {
              e.currentTarget.style.transform = 'scale(1.1)';
              e.currentTarget.style.boxShadow = `0 6px 20px ${primaryColor}50`;
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.transform = 'scale(1)';
              e.currentTarget.style.boxShadow = `0 4px 15px ${primaryColor}30`;
            }}>
              {hobby.name} • {hobby.level}
            </div>
          ))}
        </div>
      </Card>
    );
  };

  // Section renderers map
  const sectionRenderers = {
    personal: renderPersonalSection,
    education: renderEducationSection,
    jobs: renderJobsSection,
    projects: renderProjectsSection,
    hobbies: renderHobbiesSection,
  };

  return (
    <div style={{ padding: '24px' }}>
      <div style={{ width: '100%' }}>
        {sectionOrder.map((sectionKey: string) => 
          sectionRenderers[sectionKey as keyof typeof sectionRenderers]?.()
        )}
        
        {/* Settings Preview */}
        {settings && (
          <Card style={{ marginTop: '24px', background: '#fafafa' }}>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              <strong>Page Settings:</strong> {settings.pageSlug ? `pancakes.dev/personal/${settings.pageSlug}` : 'URL not set'} • 
              Color: {colorScheme} • 
              Visible sections: {Object.entries(sectionVisibility).filter(([_, visible]) => visible).length}/5
            </Text>
          </Card>
        )}
      </div>
    </div>
  );
};

// Section Settings Popover - Individual section customization
const SectionSettingsPopover: React.FC<{
  sectionKey: string;
  sectionSettings: any;
  onSettingsChange: (sectionKey: string, newSettings: any) => void;
  templateOptions: Array<{ value: string; label: string }>;
}> = ({ sectionKey, sectionSettings, onSettingsChange, templateOptions }) => {
  const colorSchemes = [
    { value: 'blue', label: '🔵 Blue', color: '#1890ff' },
    { value: 'purple', label: '🟣 Purple', color: '#722ed1' },
    { value: 'green', label: '🟢 Green', color: '#52c41a' },
    { value: 'orange', label: '🟠 Orange', color: '#fa8c16' },
    { value: 'red', label: '🔴 Red', color: '#f5222d' },
    { value: 'gold', label: '🟡 Gold', color: '#faad14' }
  ];

  const updateSetting = (key: string, value: any) => {
    onSettingsChange(sectionKey, {
      ...sectionSettings,
      [key]: value
    });
  };

  const content = (
    <div style={{ width: '280px', padding: '8px' }}>
      <Title level={5} style={{ margin: '0 0 16px', fontSize: '14px' }}>
        🎨 {sectionKey.charAt(0).toUpperCase() + sectionKey.slice(1)} Settings
      </Title>
      
      {/* Template Selection */}
      <div style={{ marginBottom: '16px' }}>
        <Text style={{ fontSize: '12px', color: '#666', marginBottom: '8px', display: 'block' }}>
          Template Style:
        </Text>
        <Select
          value={sectionSettings.template}
          onChange={(value) => updateSetting('template', value)}
          style={{ width: '100%' }}
          size="small"
        >
          {templateOptions.map(option => (
            <Option key={option.value} value={option.value}>
              {option.label}
            </Option>
          ))}
        </Select>
      </div>

      {/* Color Selection */}
      <div style={{ marginBottom: '16px' }}>
        <Text style={{ fontSize: '12px', color: '#666', marginBottom: '8px', display: 'block' }}>
          Section Color:
        </Text>
        <Row gutter={[6, 6]}>
          {colorSchemes.map(scheme => (
            <Col span={8} key={scheme.value}>
              <div
                onClick={() => updateSetting('color', scheme.value)}
                style={{
                  padding: '8px',
                  border: `2px solid ${sectionSettings.color === scheme.value ? scheme.color : '#f0f0f0'}`,
                  borderRadius: '6px',
                  cursor: 'pointer',
                  background: sectionSettings.color === scheme.value ? `${scheme.color}15` : 'white',
                  transition: 'all 0.2s',
                  textAlign: 'center',
                  fontSize: '10px'
                }}
              >
                {scheme.label}
              </div>
            </Col>
          ))}
        </Row>
      </div>

      {/* Quick Actions */}
      <div style={{ 
        paddingTop: '12px', 
        borderTop: '1px solid #f0f0f0',
        display: 'flex',
        gap: '8px'
      }}>
        <Button size="small" style={{ fontSize: '10px' }}>
          🔄 Reset
        </Button>
        <Button size="small" style={{ fontSize: '10px' }}>
          📋 Copy Style
        </Button>
        <Button size="small" type="primary" style={{ fontSize: '10px' }}>
          ✨ Preview
        </Button>
      </div>
    </div>
  );

  return (
    <Popover
      content={content}
      title={null}
      trigger="click"
      placement="bottomRight"
      overlayStyle={{ zIndex: 1050 }}
    >
      <Button
        type="primary"
        size="small"
        icon={<SettingOutlined />}
        style={{
          position: 'absolute',
          top: '16px',
          right: '16px',
          zIndex: 1000,
          background: '#1890ff',
          backdropFilter: 'blur(8px)',
          border: 'none',
          borderRadius: '8px',
          width: '32px',
          height: '32px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          boxShadow: '0 4px 12px rgba(24, 144, 255, 0.4)',
          transition: 'all 0.3s ease'
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.transform = 'scale(1.15)';
          e.currentTarget.style.boxShadow = '0 6px 20px rgba(24, 144, 255, 0.6)';
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.transform = 'scale(1)';
          e.currentTarget.style.boxShadow = '0 4px 12px rgba(24, 144, 255, 0.4)';
        }}
      />
    </Popover>
  );
};

// Live Customize Panel - Real-time settings with instant preview
const LiveCustomizePanel: React.FC<{ settings: any, onSettingsChange: (settings: any) => void }> = ({ 
  settings, onSettingsChange 
}) => {
  const colorSchemes = [
    { value: 'blue', label: '🔵 Blue Ocean', color: '#1890ff' },
    { value: 'purple', label: '🟣 Purple Magic', color: '#722ed1' },
    { value: 'green', label: '🟢 Nature Green', color: '#52c41a' },
    { value: 'orange', label: '🟠 Sunset Orange', color: '#fa8c16' },
    { value: 'red', label: '🔴 Passion Red', color: '#f5222d' },
    { value: 'gold', label: '🟡 Golden Hour', color: '#faad14' }
  ];
  
  const sectionLabels: Record<string, string> = {
    personal: '👤 Personal',
    education: '🎓 Education',
    jobs: '💼 Jobs',
    projects: '🚀 Projects',
    hobbies: '🎯 Hobbies'
  };

  const templateOptions: Record<string, Array<{ value: string; label: string }>> = {
    personal: [
      { value: 'hero', label: '🦸 Hero Banner' },
      { value: 'minimal', label: '⭕ Minimal' },
      { value: 'creative', label: '🎨 Creative' },
      { value: 'professional', label: '💼 Executive' }
    ],
    education: [
      { value: 'academic', label: '🎓 Academic Timeline' },
      { value: 'university', label: '🏛️ University Cards' },
      { value: 'progress', label: '📈 Progress Journey' },
      { value: 'certificates', label: '🏆 Achievement Gallery' },
      { value: 'simple', label: '📋 Simple List' }
    ],
    jobs: [
      { value: 'career', label: '🚀 Career Journey' },
      { value: 'corporate', label: '🏢 Corporate Timeline' },
      { value: 'experience', label: '💼 Experience Cards' },
      { value: 'skills', label: '⚡ Skills Evolution' },
      { value: 'compact', label: '📊 Compact View' }
    ],
    projects: [
      { value: 'portfolio', label: '🎯 Portfolio Showcase' },
      { value: 'github', label: '💻 GitHub Style' },
      { value: 'masonry', label: '🧱 Masonry Grid' },
      { value: 'slider', label: '🎠 Project Slider' },
      { value: 'detailed', label: '📋 Detailed List' }
    ],
    hobbies: [
      { value: 'creative', label: '🎨 Creative Cloud' },
      { value: 'interactive', label: '⚡ Interactive Skills' },
      { value: 'badges', label: '🏆 Achievement Badges' },
      { value: 'radar', label: '📊 Skill Radar' },
      { value: 'minimal', label: '🏷️ Tag Cloud' }
    ]
  };

  const updateSetting = (key: string, value: any) => {
    onSettingsChange({
      ...settings,
      [key]: value
    });
  };

  const updateSectionVisibility = (section: string, visible: boolean) => {
    const newVisibility = {
      ...settings?.sectionVisibility,
      [section]: visible
    };
    updateSetting('sectionVisibility', newVisibility);
  };

  const updateSectionTemplate = (section: string, template: string) => {
    const newTemplates = {
      ...settings?.sectionTemplates,
      [section]: template
    };
    updateSetting('sectionTemplates', newTemplates);
  };

  const moveSectionUp = (index: number) => {
    if (index > 0) {
      const newOrder = [...(settings?.sectionOrder || [])];
      [newOrder[index - 1], newOrder[index]] = [newOrder[index], newOrder[index - 1]];
      updateSetting('sectionOrder', newOrder);
    }
  };

  const moveSectionDown = (index: number) => {
    const order = settings?.sectionOrder || [];
    if (index < order.length - 1) {
      const newOrder = [...order];
      [newOrder[index], newOrder[index + 1]] = [newOrder[index + 1], newOrder[index]];
      updateSetting('sectionOrder', newOrder);
    }
  };

  return (
    <div style={{ height: '100%', overflow: 'auto' }}>
      {/* Header */}
      <div style={{ 
        padding: '16px', 
        borderBottom: '1px solid #f0f0f0',
        background: '#fafafa',
        position: 'sticky',
        top: 0,
        zIndex: 10
      }}>
        <Title level={4} style={{ margin: 0 }}>🎨 Live Customize</Title>
        <Text type="secondary" style={{ fontSize: '12px' }}>Changes apply instantly</Text>
      </div>

      <div style={{ padding: '16px' }}>
        {/* Color Scheme */}
        <div style={{ marginBottom: '24px' }}>
          <Title level={5}>🌈 Color Scheme</Title>
          <Row gutter={[8, 8]}>
            {colorSchemes.map(scheme => (
              <Col span={12} key={scheme.value}>
                <div
                  onClick={() => updateSetting('colorScheme', scheme.value)}
                  style={{
                    padding: '8px',
                    border: `2px solid ${settings?.colorScheme === scheme.value ? scheme.color : '#f0f0f0'}`,
                    borderRadius: '8px',
                    cursor: 'pointer',
                    background: settings?.colorScheme === scheme.value ? `${scheme.color}10` : 'white',
                    transition: 'all 0.2s',
                    textAlign: 'center'
                  }}
                >
                  <div style={{ fontSize: '12px' }}>{scheme.label}</div>
                </div>
              </Col>
            ))}
          </Row>
        </div>

        {/* Section Management */}
        <div>
          <Title level={5}>📋 Sections</Title>
          {(settings?.sectionOrder || []).map((section: string, index: number) => (
            <div
              key={section}
              style={{
                border: '1px solid #f0f0f0',
                borderRadius: '6px',
                padding: '12px',
                marginBottom: '8px',
                background: settings?.sectionVisibility?.[section] ? 'white' : '#fafafa'
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '8px' }}>
                <Text style={{ flex: 1, fontSize: '13px' }}>
                  {sectionLabels[section]}
                </Text>
                
                {/* Visibility Toggle */}
                <Button
                  size="small"
                  type={settings?.sectionVisibility?.[section] ? "primary" : "default"}
                  onClick={() => updateSectionVisibility(section, !settings?.sectionVisibility?.[section])}
                  style={{ width: '24px', height: '24px', padding: 0, fontSize: '10px' }}
                >
                  {settings?.sectionVisibility?.[section] ? '👁' : '🙈'}
                </Button>

                {/* Move Controls */}
                <Button
                  size="small"
                  onClick={() => moveSectionUp(index)}
                  disabled={index === 0}
                  style={{ width: '20px', height: '20px', padding: 0, fontSize: '10px' }}
                >
                  ↑
                </Button>
                <Button
                  size="small"
                  onClick={() => moveSectionDown(index)}
                  disabled={index === (settings?.sectionOrder?.length || 0) - 1}
                  style={{ width: '20px', height: '20px', padding: 0, fontSize: '10px' }}
                >
                  ↓
                </Button>
              </div>

              {/* Template Selection */}
              {templateOptions[section] && settings?.sectionVisibility?.[section] && (
                <Select
                  size="small"
                  value={settings?.sectionTemplates?.[section]}
                  onChange={(value) => updateSectionTemplate(section, value)}
                  style={{ width: '100%' }}
                >
                  {templateOptions[section].map(option => (
                    <Option key={option.value} value={option.value}>
                      {option.label}
                    </Option>
                  ))}
                </Select>
              )}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

const PersonalPage: React.FC = () => {
  const { navigate } = useRouter();
  const { settings, updateSettings, loading, error } = usePersonalPage();
  const [isCustomizePanelOpen, setIsCustomizePanelOpen] = useState(false);
  const [isPublicLoading, setIsPublicLoading] = useState(false);
  const { message } = App.useApp();
  
  // Real-time settings state (local state that updates immediately)
  const [liveSettings, setLiveSettings] = useState(settings);
  
  // Per-section settings state
  const [sectionSettings, setSectionSettings] = useState<Record<string, any>>({
    personal: { template: 'hero', color: 'blue' },
    education: { template: 'academic', color: 'blue' },
    jobs: { template: 'career', color: 'blue' },
    projects: { template: 'portfolio', color: 'blue' },
    hobbies: { template: 'creative', color: 'blue' }
  });
  
  // Sync live settings when main settings change
  React.useEffect(() => {
    if (settings) {
      setLiveSettings(settings);
      // Initialize per-section settings from main settings
      if (settings.sectionTemplates || settings.colorScheme) {
        setSectionSettings(prev => ({
          ...prev,
          personal: { 
            template: settings.sectionTemplates?.personal || 'hero', 
            color: settings.colorScheme || 'blue' 
          },
          education: { 
            template: settings.sectionTemplates?.education || 'academic', 
            color: settings.colorScheme || 'blue' 
          },
          jobs: { 
            template: settings.sectionTemplates?.jobs || 'career', 
            color: settings.colorScheme || 'blue' 
          },
          projects: { 
            template: settings.sectionTemplates?.projects || 'portfolio', 
            color: settings.colorScheme || 'blue' 
          },
          hobbies: { 
            template: settings.sectionTemplates?.hobbies || 'creative', 
            color: settings.colorScheme || 'blue' 
          }
        }));
      }
    }
  }, [settings]);

  // Handle per-section settings changes
  const handleSectionSettingsChange = (sectionKey: string, newSettings: any) => {
    setSectionSettings(prev => ({
      ...prev,
      [sectionKey]: newSettings
    }));
    
    // TODO: Add persistence for per-section settings
    // For now, just update local state for immediate visual feedback
  };

  const handleTogglePublic = async (checked: boolean): Promise<void> => {
    try {
      setIsPublicLoading(true);
      await updateSettings({ isPublic: checked });
      message.success(checked ? 'Personal page is now public!' : 'Personal page is now private');
    } catch (error) {
      console.error('Error updating visibility:', error);
      message.error('Failed to update page visibility');
    } finally {
      setIsPublicLoading(false);
    }
  };

  const handleShare = () => {
    if (settings?.pageSlug) {
      const url = `${window.location.origin}/personal/${settings.pageSlug}`;
      navigator.clipboard.writeText(url);
      message.success('Page URL copied to clipboard!');
    }
  };

  if (loading) {
    return (
      <div className="personal-page__loading">
        <Spin size="large" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="personal-page__error">
        <Alert
          message="Error Loading Personal Page"
          description={error}
          type="error"
          showIcon
        />
      </div>
    );
  }

  return (
    <div className="personal-page">
      <div className="personal-page__header">
        <Card className="personal-page__header-card">
          <div className="personal-page__header-content">
            <div className="personal-page__header-left">
              <Button 
                icon={<ArrowLeftOutlined />}
                onClick={() => navigate('profile')}
                className="personal-page__back-btn"
              >
                Back to Profile
              </Button>
              <div className="personal-page__title-section">
                <Title level={2} className="personal-page__title">
                  My Personal Page
                </Title>
                <Text type="secondary">
                  {settings?.pageSlug && `pancakes.dev/personal/${settings.pageSlug}`}
                </Text>
              </div>
            </div>
            
            <div className="personal-page__header-right">
              <Space>
                <div className="personal-page__visibility">
                  <Text>Public:</Text>
                  <Switch
                    checked={settings?.isPublic || false}
                    onChange={handleTogglePublic}
                    loading={isPublicLoading}
                  />
                </div>
                
                {settings?.isPublic && (
                  <Button 
                    icon={<ShareAltOutlined />}
                    onClick={handleShare}
                  >
                    Share
                  </Button>
                )}
                
                                <Button 
                  type={isCustomizePanelOpen ? "primary" : "default"}
                  icon={<SettingOutlined />}
                  onClick={() => setIsCustomizePanelOpen(!isCustomizePanelOpen)}
                >
                  {isCustomizePanelOpen ? 'Close Customize' : 'Customize'}
                </Button>
              </Space>
            </div>
          </div>
        </Card>
      </div>

      <div className="personal-page__content" style={{ display: 'flex', gap: '24px', position: 'relative' }}>
        {/* Main content - always visible */}
        <div style={{ flex: 1 }}>
          <PersonalPageView 
            settings={liveSettings || settings}
            sectionSettings={sectionSettings}
            onSectionSettingsChange={handleSectionSettingsChange}
          />
        </div>
        
        {/* Floating Customize Panel */}
        {isCustomizePanelOpen && (
          <div style={{
            position: 'fixed',
            top: '80px',
            right: '24px',
            width: '350px',
            maxHeight: 'calc(100vh - 120px)',
            background: 'white',
            borderRadius: '12px',
            boxShadow: '0 8px 30px rgba(0, 0, 0, 0.12)',
            border: '1px solid #f0f0f0',
            zIndex: 1000,
            overflow: 'hidden'
          }}>
                         <LiveCustomizePanel
               settings={liveSettings || settings}
               onSettingsChange={(newSettings: any) => {
                 setLiveSettings(newSettings);
                 // Auto-save after a short delay
                 setTimeout(() => updateSettings(newSettings), 1000);
               }}
             />
          </div>
        )}
      </div>
    </div>
  );
};

const PersonalPageWithProvider: React.FC = () => (
  <App>
    <PersonalPage />
  </App>
);

export default PersonalPageWithProvider; 