import React from 'react';
import { Card, Typography, Row, Col, Tag } from 'antd';
import SectionSettingsPopover from '../SectionSettingsPopover';
import type { SectionRendererProps } from '../../types';
import { SECTION_COLORS } from '../../constants';

const { Title, Text, Paragraph } = Typography;

interface EducationSectionProps extends SectionRendererProps {
  educations?: any[];
}

const EducationSection: React.FC<EducationSectionProps> = ({
  sectionKey,
  user,
  data,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
  educations,
}) => {
  // Use either data prop or educations prop
  const educationData = educations || data || [];
  const { template } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

  const renderAcademicTemplate = () => (
    <Card key="education" style={{ 
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
        🎓 Academic Timeline
      </Title>
      
             <Row gutter={[24, 24]}>
         {educationData.map((edu: any, index: number) => (
          <Col key={index} xs={24} md={12}>
            <div style={{
              background: 'linear-gradient(135deg, #f8f9fa 0%, #ffffff 100%)',
              borderRadius: '16px',
              padding: '24px',
              height: '100%',
              border: `2px solid ${sectionPrimaryColor}15`,
              position: 'relative',
              overflow: 'hidden'
            }}>
              <div style={{
                position: 'absolute',
                top: 0,
                left: 0,
                width: '100%',
                height: '4px',
                background: `linear-gradient(90deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`
              }} />
              
              <div style={{ textAlign: 'center', marginBottom: '20px' }}>
                <div style={{
                  background: `${sectionPrimaryColor}15`,
                  borderRadius: '50%',
                  width: '60px',
                  height: '60px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  margin: '0 auto 12px',
                  fontSize: '24px'
                }}>
                  🎓
                </div>
              </div>
              
              <Title level={4} style={{ 
                margin: 0, 
                color: sectionPrimaryColor,
                fontSize: '18px',
                textAlign: 'center'
              }}>
                {edu.institution}
              </Title>
              
              <div style={{
                background: 'rgba(255,255,255,0.8)',
                padding: '16px',
                borderRadius: '12px',
                marginBottom: '16px',
                border: `1px solid ${sectionPrimaryColor}15`
              }}>
                <Text strong style={{ fontSize: '15px', color: '#333' }}>
                  {edu.degree}
                </Text>
                <br />
                <Text style={{ fontSize: '14px', color: sectionPrimaryColor }}>
                  Specialization: {edu.specialization}
                </Text>
              </div>
              
              <div style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginBottom: '12px'
              }}>
                <Tag color={sectionPrimaryColor}>{edu.startDate}</Tag>
                <span style={{ color: '#999' }}>→</span>
                <Tag color={sectionPrimaryColor}>{edu.endDate || 'Present'}</Tag>
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

  const renderGridTemplate = () => (
    <Card key="education" style={{ 
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
      
      <Title level={2} style={{ color: sectionPrimaryColor, marginBottom: '32px' }}>
        📋 Education Grid
      </Title>
      
             <Row gutter={[20, 20]}>
         {educationData.map((edu: any, index: number) => (
          <Col key={index} xs={24} sm={12} lg={8}>
            <Card 
              size="small"
              style={{
                height: '100%',
                border: `1px solid ${sectionPrimaryColor}30`,
                borderRadius: '12px'
              }}
              bodyStyle={{ padding: '16px' }}
            >
              <div style={{ textAlign: 'center', marginBottom: '12px' }}>
                <Title level={5} style={{ margin: 0, color: sectionPrimaryColor }}>
                  {edu.institution}
                </Title>
              </div>
              <Text strong style={{ fontSize: '14px' }}>{edu.degree}</Text>
              <br />
              <Text style={{ fontSize: '13px', color: '#666' }}>{edu.specialization}</Text>
              <div style={{ marginTop: '12px', textAlign: 'center' }}>
                <Tag color={sectionPrimaryColor}>
                  {edu.startDate} - {edu.endDate || 'Present'}
                </Tag>
              </div>
            </Card>
          </Col>
        ))}
      </Row>
    </Card>
  );

  const renderTimelineTemplate = () => (
    <Card key="education" style={{ marginBottom: '24px', position: 'relative' }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
             <Title level={2} style={{ color: sectionPrimaryColor }}>🎓 Education</Title>
       
       {educationData.map((edu: any, index: number) => (
         <div key={index} style={{ 
           padding: '16px 0', 
           borderBottom: index < educationData.length - 1 ? `1px solid ${sectionPrimaryColor}20` : 'none' 
        }}>
          <Text strong style={{ fontSize: '16px' }}>{edu.institution}</Text>
          <br />
          <Text>{edu.degree} in {edu.specialization}</Text>
          <br />
          <Text type="secondary">{edu.startDate} - {edu.endDate || 'Present'}</Text>
          {edu.description && (
            <Paragraph style={{ marginTop: '8px', fontSize: '14px', color: '#666' }}>
              {edu.description}
            </Paragraph>
          )}
        </div>
      ))}
    </Card>
  );

  const renderJourneyTemplate = () => (
    <Card key="education" style={{ 
      marginBottom: '32px', 
      borderRadius: '16px',
      position: 'relative',
      overflow: 'hidden'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor, textAlign: 'center', marginBottom: '32px' }}>
        🛤️ Education Journey
      </Title>
      
      {/* Journey Path */}
      <div style={{ position: 'relative', padding: '20px' }}>
        {educationData.map((edu: any, index: number) => (
          <div key={index} style={{ position: 'relative', marginBottom: '40px' }}>
            {/* Journey Line */}
            {index < educationData.length - 1 && (
              <div style={{
                position: 'absolute',
                left: '50%',
                top: '80px',
                transform: 'translateX(-50%)',
                width: '3px',
                height: '60px',
                background: `linear-gradient(to bottom, ${sectionPrimaryColor}, ${sectionPrimaryColor}50)`,
                zIndex: 1
              }} />
            )}
            
            <Row align="middle" gutter={[24, 24]}>
              <Col xs={24} md={12} style={{ order: index % 2 === 0 ? 1 : 2 }}>
                <div style={{
                  background: 'linear-gradient(135deg, #fff 0%, #f8f9fa 100%)',
                  borderRadius: '16px',
                  padding: '24px',
                  border: `2px solid ${sectionPrimaryColor}20`,
                  position: 'relative',
                  boxShadow: '0 8px 25px rgba(0,0,0,0.1)'
                }}>
                  <div style={{
                    position: 'absolute',
                    top: '-15px',
                    left: '20px',
                    background: sectionPrimaryColor,
                    color: 'white',
                    padding: '8px 16px',
                    borderRadius: '20px',
                    fontSize: '12px',
                    fontWeight: '600'
                  }}>
                    {edu.startDate} - {edu.endDate || 'Present'}
                  </div>
                  
                  <Title level={4} style={{ color: sectionPrimaryColor, marginTop: '10px', marginBottom: '8px' }}>
                    {edu.institution}
                  </Title>
                  <Text strong style={{ fontSize: '16px', color: '#333' }}>{edu.degree}</Text>
                  <br />
                  <Text style={{ fontSize: '14px', color: '#666' }}>{edu.specialization}</Text>
                  
                  {edu.description && (
                    <Paragraph style={{ 
                      marginTop: '12px',
                      fontSize: '13px', 
                      color: '#666',
                      lineHeight: 1.5
                    }}>
                      {edu.description}
                    </Paragraph>
                  )}
                </div>
              </Col>
              
              <Col xs={24} md={12} style={{ 
                order: index % 2 === 0 ? 2 : 1,
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center'
              }}>
                <div style={{
                  width: '80px',
                  height: '80px',
                  background: `linear-gradient(45deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`,
                  borderRadius: '50%',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '28px',
                  boxShadow: '0 8px 20px rgba(0,0,0,0.15)',
                  position: 'relative',
                  zIndex: 2
                }}>
                  🎓
                </div>
              </Col>
            </Row>
          </div>
        ))}
      </div>
    </Card>
  );

  const renderUniversityTemplate = () => (
    <Card key="education" style={{ 
      marginBottom: '32px', 
      borderRadius: '20px',
      position: 'relative',
      background: 'linear-gradient(135deg, #f8f9fa 0%, #ffffff 100%)',
      border: 'none',
      boxShadow: '0 15px 35px rgba(0,0,0,0.1)'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      {/* University Header */}
      <div style={{
        background: `linear-gradient(135deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}dd)`,
        padding: '32px',
        textAlign: 'center',
        borderRadius: '20px 20px 0 0',
        position: 'relative'
      }}>
        <div style={{
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'url("data:image/svg+xml,%3Csvg width="60" height="60" viewBox="0 0 60 60" xmlns="http://www.w3.org/2000/svg"%3E%3Cg fill="none" fill-rule="evenodd"%3E%3Cg fill="%23ffffff" fill-opacity="0.1"%3E%3Ccircle cx="6" cy="6" r="3"/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")',
          opacity: 0.3
        }} />
        <Title level={2} style={{ color: 'white', margin: 0, fontSize: '28px', fontWeight: '700' }}>
          🏛️ University Style
        </Title>
        <Text style={{ color: 'rgba(255,255,255,0.9)', fontSize: '16px', marginTop: '8px' }}>
          Academic Excellence Journey
        </Text>
      </div>
      
      <div style={{ padding: '32px' }}>
        {educationData.map((edu: any, index: number) => (
          <div key={index} style={{
            background: '#fff',
            borderRadius: '16px',
            padding: '28px',
            marginBottom: index < educationData.length - 1 ? '24px' : 0,
            border: `1px solid ${sectionPrimaryColor}15`,
            position: 'relative',
            boxShadow: '0 4px 15px rgba(0,0,0,0.05)'
          }}>
            {/* University Badge */}
            <div style={{
              position: 'absolute',
              top: '-12px',
              right: '20px',
              background: sectionPrimaryColor,
              color: 'white',
              padding: '6px 12px',
              borderRadius: '12px',
              fontSize: '11px',
              fontWeight: '600',
              textTransform: 'uppercase'
            }}>
              #{index + 1}
            </div>
            
            <Row gutter={[24, 16]}>
              <Col xs={24} sm={16}>
                <div>
                  <Title level={3} style={{ 
                    color: sectionPrimaryColor, 
                    marginBottom: '8px',
                    fontSize: '20px'
                  }}>
                    {edu.institution}
                  </Title>
                  
                  <div style={{
                    background: `${sectionPrimaryColor}08`,
                    padding: '12px 16px',
                    borderRadius: '8px',
                    marginBottom: '12px',
                    borderLeft: `4px solid ${sectionPrimaryColor}`
                  }}>
                    <Text strong style={{ fontSize: '15px', color: '#333' }}>
                      {edu.degree}
                    </Text>
                    <br />
                    <Text style={{ fontSize: '13px', color: sectionPrimaryColor, fontWeight: '500' }}>
                      Major: {edu.specialization}
                    </Text>
                  </div>
                  
                  {edu.description && (
                    <Paragraph style={{ 
                      fontSize: '14px', 
                      color: '#666',
                      lineHeight: 1.6,
                      margin: 0
                    }}>
                      {edu.description}
                    </Paragraph>
                  )}
                </div>
              </Col>
              
              <Col xs={24} sm={8}>
                <div style={{ textAlign: 'center' }}>
                  <div style={{
                    background: `linear-gradient(135deg, ${sectionPrimaryColor}15, ${sectionPrimaryColor}25)`,
                    borderRadius: '50%',
                    width: '70px',
                    height: '70px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    margin: '0 auto 12px',
                    fontSize: '24px'
                  }}>
                    🎓
                  </div>
                  <div style={{
                    background: `${sectionPrimaryColor}10`,
                    padding: '8px 12px',
                    borderRadius: '20px',
                    display: 'inline-block'
                  }}>
                    <Text style={{ fontSize: '12px', color: sectionPrimaryColor, fontWeight: '600' }}>
                      {edu.startDate} - {edu.endDate || 'Present'}
                    </Text>
                  </div>
                </div>
              </Col>
            </Row>
          </div>
        ))}
      </div>
    </Card>
  );

  const renderProgressTemplate = () => (
    <Card key="education" style={{ 
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
        📊 Progress Timeline
      </Title>
      
      <div style={{ padding: '20px' }}>
        {/* Progress Line */}
        <div style={{
          position: 'relative',
          marginLeft: '40px',
          paddingLeft: '40px'
        }}>
          <div style={{
            position: 'absolute',
            left: '20px',
            top: '0',
            bottom: '0',
            width: '3px',
            background: `linear-gradient(to bottom, ${sectionPrimaryColor}, ${sectionPrimaryColor}40)`,
            borderRadius: '2px'
          }} />
          
          {educationData.map((edu: any, index: number) => (
            <div key={index} style={{ 
              position: 'relative',
              marginBottom: '32px',
              paddingBottom: '20px'
            }}>
              {/* Progress Dot */}
              <div style={{
                position: 'absolute',
                left: '-28px',
                top: '10px',
                width: '16px',
                height: '16px',
                background: sectionPrimaryColor,
                borderRadius: '50%',
                border: '4px solid white',
                boxShadow: '0 2px 8px rgba(0,0,0,0.15)',
                zIndex: 2
              }} />
              
              {/* Progress Indicator */}
              <div style={{
                position: 'absolute',
                left: '-50px',
                top: '8px',
                width: '60px',
                height: '20px',
                background: `${sectionPrimaryColor}15`,
                borderRadius: '10px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center'
              }}>
                <Text style={{ fontSize: '10px', color: sectionPrimaryColor, fontWeight: '600' }}>
                  {Math.round(((index + 1) / educationData.length) * 100)}%
                </Text>
              </div>
              
              <div style={{
                background: '#fff',
                borderRadius: '12px',
                padding: '20px',
                border: `1px solid ${sectionPrimaryColor}20`,
                boxShadow: '0 4px 12px rgba(0,0,0,0.05)',
                marginLeft: '20px'
              }}>
                <Row gutter={[16, 12]}>
                  <Col xs={24} sm={18}>
                    <Title level={4} style={{ 
                      color: sectionPrimaryColor, 
                      marginBottom: '4px',
                      fontSize: '16px'
                    }}>
                      {edu.institution}
                    </Title>
                    <Text strong style={{ fontSize: '14px' }}>{edu.degree}</Text>
                    <br />
                    <Text style={{ fontSize: '13px', color: '#666' }}>{edu.specialization}</Text>
                    
                    {edu.description && (
                      <Paragraph style={{ 
                        marginTop: '8px',
                        fontSize: '12px', 
                        color: '#666',
                        lineHeight: 1.4,
                        margin: '8px 0 0'
                      }}>
                        {edu.description}
                      </Paragraph>
                    )}
                  </Col>
                  
                  <Col xs={24} sm={6}>
                    <div style={{ textAlign: 'right' }}>
                      <div style={{
                        background: `${sectionPrimaryColor}10`,
                        padding: '6px 10px',
                        borderRadius: '6px',
                        display: 'inline-block'
                      }}>
                        <Text style={{ fontSize: '11px', color: sectionPrimaryColor, fontWeight: '600' }}>
                          {edu.startDate}
                        </Text>
                      </div>
                      <br />
                      <div style={{
                        background: `${sectionPrimaryColor}20`,
                        padding: '6px 10px',
                        borderRadius: '6px',
                        display: 'inline-block',
                        marginTop: '4px'
                      }}>
                        <Text style={{ fontSize: '11px', color: sectionPrimaryColor, fontWeight: '600' }}>
                          {edu.endDate || 'Present'}
                        </Text>
                      </div>
                    </div>
                  </Col>
                </Row>
              </div>
            </div>
          ))}
        </div>
      </div>
    </Card>
  );

  // Template selector
  switch (template) {
    case 'academic':
      return renderAcademicTemplate();
    case 'grid':
      return renderGridTemplate();
    case 'journey':
      return renderJourneyTemplate();
    case 'university':
      return renderUniversityTemplate();
    case 'progress':
      return renderProgressTemplate();
    case 'timeline':
      return renderTimelineTemplate();
    default:
      return renderTimelineTemplate(); 
  }
};

export default EducationSection; 