import React from 'react';
import { Card, Typography, Row, Col, Tag } from 'antd';
import SectionSettingsPopover from '../SectionSettingsPopover';
import type { SectionRendererProps } from '../../types';
import { SECTION_COLORS } from '../../constants';

const { Title, Text } = Typography;

interface HobbiesSectionProps extends SectionRendererProps {
  hobbies?: any[];
}

const HobbiesSection: React.FC<HobbiesSectionProps> = ({
  sectionKey,
  user,
  data,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
  hobbies,
}) => {
  // Use either data prop or hobbies prop
  const hobbiesData = hobbies || data || [];
  const { template } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

  const renderCreativeTemplate = () => (
    <Card key="hobbies" style={{ 
      marginBottom: '32px', 
      borderRadius: '16px',
      position: 'relative'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor, textAlign: 'center', marginBottom: '32px' }}>
        🎨 Creative Layout
      </Title>
      
      <Row gutter={[20, 20]}>
        {hobbiesData.map((hobby: any, index: number) => (
          <Col key={index} xs={24} sm={12} md={8} lg={6}>
            <div style={{
              background: `linear-gradient(135deg, ${sectionPrimaryColor}15, ${sectionPrimaryColor}08)`,
              borderRadius: '16px',
              padding: '20px',
              textAlign: 'center',
              border: `2px solid ${sectionPrimaryColor}20`,
              position: 'relative',
              overflow: 'hidden',
              minHeight: '120px',
              display: 'flex',
              flexDirection: 'column',
              justifyContent: 'center'
            }}>
              <div style={{
                position: 'absolute',
                top: 0,
                left: 0,
                width: '100%',
                height: '4px',
                background: `linear-gradient(90deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`
              }} />
              
              <div style={{
                background: `${sectionPrimaryColor}20`,
                borderRadius: '50%',
                width: '50px',
                height: '50px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                margin: '0 auto 12px',
                fontSize: '20px'
              }}>
                🎯
              </div>
              
              <Title level={5} style={{ 
                color: sectionPrimaryColor, 
                margin: '0 0 8px',
                fontSize: '14px'
              }}>
                {hobby.name}
              </Title>
              
              <Text style={{ 
                fontSize: '12px', 
                color: '#666',
                lineHeight: 1.4
              }}>
                {hobby.description}
              </Text>
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  );

  const renderTagsTemplate = () => (
    <Card key="hobbies" style={{ 
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
      
      <Title level={2} style={{ color: sectionPrimaryColor, marginBottom: '24px' }}>
        🏷️ Interests & Hobbies
      </Title>
      
      <div style={{ 
        display: 'flex', 
        flexWrap: 'wrap', 
        justifyContent: 'center', 
        gap: '12px' 
      }}>
        {hobbiesData.map((hobby: any, index: number) => (
          <Tag
            key={index}
            style={{
              fontSize: '14px',
              padding: '8px 16px',
              borderRadius: '20px',
              border: `2px solid ${sectionPrimaryColor}30`,
              background: `${sectionPrimaryColor}10`,
              color: sectionPrimaryColor,
              fontWeight: '500'
            }}
          >
            {hobby.name}
          </Tag>
        ))}
      </div>
    </Card>
  );

  const renderMinimalTemplate = () => (
    <Card key="hobbies" style={{ 
      marginBottom: '24px',
      position: 'relative'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor, marginBottom: '20px' }}>
        ✨ Simple Tags
      </Title>
      
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
        {hobbiesData.map((hobby: any, index: number) => (
          <Tag key={index} color={sectionPrimaryColor}>
            {hobby.name}
          </Tag>
        ))}
      </div>
    </Card>
  );

  const renderInterestsTemplate = () => (
    <Card key="hobbies" style={{ 
      marginBottom: '32px', 
      borderRadius: '20px',
      position: 'relative',
      background: 'linear-gradient(135deg, #f8f9fa 0%, #ffffff 100%)',
      border: 'none',
      boxShadow: '0 20px 40px rgba(0,0,0,0.1)'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      {/* Interests Header */}
      <div style={{
        background: `linear-gradient(135deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}dd)`,
        padding: '32px',
        textAlign: 'center',
        borderRadius: '20px 20px 0 0',
        position: 'relative'
      }}>
        <Title level={2} style={{ color: 'white', margin: 0, fontSize: '28px', fontWeight: '700' }}>
          🎯 Interest Cards
        </Title>
        <Text style={{ color: 'rgba(255,255,255,0.9)', fontSize: '16px', marginTop: '8px' }}>
          What Drives My Passion
        </Text>
      </div>
      
      <div style={{ padding: '32px' }}>
        <Row gutter={[20, 20]}>
          {hobbiesData.map((hobby: any, index: number) => (
            <Col key={index} xs={24} sm={12} md={8} lg={6}>
              <div style={{
                background: '#fff',
                borderRadius: '16px',
                padding: '24px',
                textAlign: 'center',
                border: `2px solid ${sectionPrimaryColor}15`,
                position: 'relative',
                overflow: 'hidden',
                minHeight: '180px',
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'center',
                boxShadow: '0 8px 25px rgba(0,0,0,0.08)',
                transition: 'transform 0.3s ease, box-shadow 0.3s ease'
              }}>
                {/* Interest Level Indicator */}
                <div style={{
                  position: 'absolute',
                  top: 0,
                  left: 0,
                  right: 0,
                  height: '4px',
                  background: `linear-gradient(90deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`
                }} />
                
                {/* Interest Badge */}
                <div style={{
                  position: 'absolute',
                  top: '12px',
                  right: '12px',
                  background: `${sectionPrimaryColor}15`,
                  color: sectionPrimaryColor,
                  padding: '4px 8px',
                  borderRadius: '8px',
                  fontSize: '10px',
                  fontWeight: '600'
                }}>
                  #{index + 1}
                </div>
                
                <div style={{
                  background: `linear-gradient(135deg, ${sectionPrimaryColor}15, ${sectionPrimaryColor}25)`,
                  borderRadius: '50%',
                  width: '70px',
                  height: '70px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  margin: '0 auto 16px',
                  fontSize: '28px'
                }}>
                  🎯
                </div>
                
                <Title level={5} style={{ 
                  color: sectionPrimaryColor, 
                  margin: '0 0 8px',
                  fontSize: '16px',
                  fontWeight: '600'
                }}>
                  {hobby.name}
                </Title>
                
                {hobby.description && (
                  <Text style={{ 
                    fontSize: '12px', 
                    color: '#666',
                    lineHeight: 1.5
                  }}>
                    {hobby.description}
                  </Text>
                )}
                
                {/* Interest Rating */}
                <div style={{ marginTop: '12px' }}>
                  <div style={{
                    display: 'flex',
                    justifyContent: 'center',
                    gap: '2px'
                  }}>
                    {[...Array(5)].map((_, starIndex) => (
                      <span key={starIndex} style={{
                        fontSize: '12px',
                        color: starIndex < (5 - (index % 5)) ? sectionPrimaryColor : '#ddd'
                      }}>⭐</span>
                    ))}
                  </div>
                </div>
              </div>
            </Col>
          ))}
        </Row>
      </div>
    </Card>
  );

  const renderColorfulTemplate = () => (
    <Card key="hobbies" style={{ 
      marginBottom: '32px', 
      borderRadius: '16px',
      position: 'relative',
      background: 'linear-gradient(135deg, #fff 0%, #f8f9fa 100%)'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor, textAlign: 'center', marginBottom: '32px' }}>
        🌈 Colorful Display
      </Title>
      
      <div style={{ padding: '20px' }}>
        <div style={{
          display: 'flex',
          flexWrap: 'wrap',
          justifyContent: 'center',
          gap: '16px'
        }}>
          {hobbiesData.map((hobby: any, index: number) => {
            const colors = [
              '#ff6b6b', '#4ecdc4', '#45b7d1', '#96ceb4', '#ffeaa7',
              '#dda0dd', '#98d8c8', '#f7dc6f', '#bb8fce', '#85c1e9'
            ];
            const bgColor = colors[index % colors.length];
            
            return (
              <div key={index} style={{
                background: `linear-gradient(135deg, ${bgColor}20, ${bgColor}40)`,
                borderRadius: '20px',
                padding: '20px',
                textAlign: 'center',
                border: `3px solid ${bgColor}`,
                position: 'relative',
                minWidth: '140px',
                minHeight: '140px',
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'center',
                boxShadow: `0 8px 25px ${bgColor}30`,
                transition: 'transform 0.3s ease, box-shadow 0.3s ease',
                cursor: 'pointer'
              }}>
                {/* Colorful Pattern */}
                <div style={{
                  position: 'absolute',
                  top: '-5px',
                  right: '-5px',
                  width: '30px',
                  height: '30px',
                  background: bgColor,
                  borderRadius: '50%',
                  opacity: 0.7
                }} />
                
                <div style={{
                  background: `${bgColor}40`,
                  borderRadius: '50%',
                  width: '50px',
                  height: '50px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  margin: '0 auto 12px',
                  fontSize: '20px',
                  border: `2px solid ${bgColor}`
                }}>
                  {index % 6 === 0 ? '🎨' : index % 6 === 1 ? '🎵' : index % 6 === 2 ? '📚' : 
                   index % 6 === 3 ? '⚽' : index % 6 === 4 ? '🎮' : '✈️'}
                </div>
                
                <Title level={5} style={{ 
                  color: bgColor, 
                  margin: '0 0 4px',
                  fontSize: '14px',
                  fontWeight: '700',
                  textShadow: '0 1px 2px rgba(255,255,255,0.8)'
                }}>
                  {hobby.name}
                </Title>
                
                {hobby.description && (
                  <Text style={{ 
                    fontSize: '10px', 
                    color: '#555',
                    lineHeight: 1.3,
                    fontWeight: '500'
                  }}>
                    {hobby.description.length > 30 ? hobby.description.substring(0, 30) + '...' : hobby.description}
                  </Text>
                )}
              </div>
            );
          })}
        </div>
      </div>
    </Card>
  );

  const renderIconsTemplate = () => (
    <Card key="hobbies" style={{ 
      marginBottom: '32px', 
      borderRadius: '16px',
      position: 'relative'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor, textAlign: 'center', marginBottom: '32px' }}>
        🎪 Icon Gallery
      </Title>
      
      <div style={{ padding: '20px' }}>
        <Row gutter={[24, 24]}>
          {hobbiesData.map((hobby: any, index: number) => (
            <Col key={index} xs={12} sm={8} md={6} lg={4}>
              <div style={{
                background: 'linear-gradient(135deg, #fff 0%, #f8f9fa 100%)',
                borderRadius: '20px',
                padding: '20px',
                textAlign: 'center',
                border: `2px solid ${sectionPrimaryColor}15`,
                position: 'relative',
                overflow: 'hidden',
                minHeight: '160px',
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'center',
                boxShadow: '0 8px 20px rgba(0,0,0,0.08)',
                transition: 'all 0.3s ease',
                cursor: 'pointer'
              }}>
                {/* Animated Background Pattern */}
                <div style={{
                  position: 'absolute',
                  top: '-20px',
                  right: '-20px',
                  width: '60px',
                  height: '60px',
                  background: `radial-gradient(circle, ${sectionPrimaryColor}15, transparent)`,
                  borderRadius: '50%'
                }} />
                
                {/* Icon Container */}
                <div style={{
                  background: `linear-gradient(135deg, ${sectionPrimaryColor}15, ${sectionPrimaryColor}25)`,
                  borderRadius: '20px',
                  width: '70px',
                  height: '70px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  margin: '0 auto 16px',
                  fontSize: '32px',
                  border: `3px solid ${sectionPrimaryColor}20`,
                  position: 'relative',
                  boxShadow: '0 4px 15px rgba(0,0,0,0.1)'
                }}>
                  {/* Dynamic Icon based on hobby name/index */}
                  {hobby.name.toLowerCase().includes('music') || hobby.name.toLowerCase().includes('song') ? '🎵' :
                   hobby.name.toLowerCase().includes('read') || hobby.name.toLowerCase().includes('book') ? '📚' :
                   hobby.name.toLowerCase().includes('sport') || hobby.name.toLowerCase().includes('football') ? '⚽' :
                   hobby.name.toLowerCase().includes('game') || hobby.name.toLowerCase().includes('gaming') ? '🎮' :
                   hobby.name.toLowerCase().includes('travel') || hobby.name.toLowerCase().includes('trip') ? '✈️' :
                   hobby.name.toLowerCase().includes('cook') || hobby.name.toLowerCase().includes('food') ? '🍳' :
                   hobby.name.toLowerCase().includes('art') || hobby.name.toLowerCase().includes('paint') ? '🎨' :
                   hobby.name.toLowerCase().includes('photo') || hobby.name.toLowerCase().includes('camera') ? '📷' :
                   hobby.name.toLowerCase().includes('code') || hobby.name.toLowerCase().includes('programming') ? '💻' :
                   hobby.name.toLowerCase().includes('movie') || hobby.name.toLowerCase().includes('film') ? '🎬' :
                   index % 10 === 0 ? '🎯' : index % 10 === 1 ? '🎨' : index % 10 === 2 ? '🎵' :
                   index % 10 === 3 ? '📚' : index % 10 === 4 ? '⚽' : index % 10 === 5 ? '🎮' :
                   index % 10 === 6 ? '✈️' : index % 10 === 7 ? '🍳' : index % 10 === 8 ? '📷' : '💻'}
                  
                  {/* Icon Badge */}
                  <div style={{
                    position: 'absolute',
                    top: '-8px',
                    right: '-8px',
                    width: '20px',
                    height: '20px',
                    background: sectionPrimaryColor,
                    borderRadius: '50%',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    color: 'white',
                    fontSize: '8px',
                    fontWeight: '700'
                  }}>
                    {index + 1}
                  </div>
                </div>
                
                <Title level={5} style={{ 
                  color: sectionPrimaryColor, 
                  margin: '0 0 8px',
                  fontSize: '14px',
                  fontWeight: '600',
                  lineHeight: 1.2
                }}>
                  {hobby.name}
                </Title>
                
                {hobby.description && (
                  <Text style={{ 
                    fontSize: '11px', 
                    color: '#666',
                    lineHeight: 1.4
                  }}>
                    {hobby.description.length > 40 ? hobby.description.substring(0, 40) + '...' : hobby.description}
                  </Text>
                )}
                
                {/* Hobby Level Indicator */}
                <div style={{
                  marginTop: '8px',
                  display: 'flex',
                  justifyContent: 'center',
                  gap: '3px'
                }}>
                  {[...Array(3)].map((_, dotIndex) => (
                    <div key={dotIndex} style={{
                      width: '6px',
                      height: '6px',
                      borderRadius: '50%',
                      background: dotIndex < ((index % 3) + 1) ? sectionPrimaryColor : '#ddd'
                    }} />
                  ))}
                </div>
              </div>
            </Col>
          ))}
        </Row>
      </div>
    </Card>
  );

  // Template selector
  switch (template) {
    case 'creative':
      return renderCreativeTemplate();
    case 'tags':
      return renderTagsTemplate();
    case 'interests':
      return renderInterestsTemplate();
    case 'colorful':
      return renderColorfulTemplate();
    case 'icons':
      return renderIconsTemplate();
    case 'minimal':
      return renderMinimalTemplate();
    default:
      return renderTagsTemplate(); // fallback
  }
};

export default HobbiesSection; 