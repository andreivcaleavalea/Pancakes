import React from 'react';
import { Card, Typography, Row, Col, Tag } from 'antd';
import SectionSettingsPopover from '../SectionSettingsPopover';
import type { SectionRendererProps } from '../../types';
import { SECTION_COLORS } from '../../constants';

const { Title, Text, Paragraph } = Typography;

interface JobsSectionProps extends SectionRendererProps {
  jobs?: any[];
}

const JobsSection: React.FC<JobsSectionProps> = ({
  sectionKey,
  user,
  data,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
  jobs,
}) => {
  // Use either data prop or jobs prop
  const jobsData = jobs || data || [];
  const { template } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

  const renderCareerTemplate = () => (
    <Card key="jobs" style={{ 
      marginBottom: '32px',
      position: 'relative'
    }}>
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
        {jobsData.map((job: any, index: number) => (
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

  const renderCorporateTemplate = () => (
    <Card key="jobs" style={{ marginBottom: '32px', borderRadius: '16px' }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor, textAlign: 'center', marginBottom: '32px' }}>
        🏢 Corporate Timeline
      </Title>
      
      <Row gutter={[24, 24]}>
        {jobsData.map((job: any, index: number) => (
          <Col key={index} xs={24} sm={12} md={8}>
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
                  🏢
                </div>
                <Tag color={sectionPrimaryColor} style={{ fontSize: '12px' }}>
                  {job.startDate} - {job.endDate || 'Present'}
                </Tag>
              </div>
              
              <Title level={4} style={{ 
                color: '#2c3e50', 
                textAlign: 'center',
                marginBottom: '8px',
                fontSize: '16px'
              }}>
                {job.position}
              </Title>
              
              <Text strong style={{ 
                display: 'block',
                textAlign: 'center',
                color: sectionPrimaryColor,
                marginBottom: '12px',
                fontSize: '14px'
              }}>
                {job.company}
              </Text>
              
              {job.location && (
                <Text style={{ 
                  display: 'block',
                  textAlign: 'center',
                  fontSize: '12px', 
                  color: '#666', 
                  marginBottom: '12px' 
                }}>
                  📍 {job.location}
                </Text>
              )}
              
              {job.description && (
                <Paragraph style={{ 
                  fontSize: '13px', 
                  color: '#555',
                  lineHeight: 1.5,
                  margin: 0,
                  textAlign: 'center'
                }}>
                  {job.description.length > 100 ? job.description.substring(0, 100) + '...' : job.description}
                </Paragraph>
              )}
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  );

  const renderTimelineTemplate = () => (
    <Card key="jobs" style={{ marginBottom: '24px', position: 'relative' }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor }}>💼 Work Experience</Title>
      
      {jobsData.map((job: any, index: number) => (
        <div key={index} style={{ 
          padding: '20px 0', 
          borderBottom: index < jobsData.length - 1 ? `1px solid ${sectionPrimaryColor}20` : 'none' 
        }}>
          <Title level={4} style={{ margin: '0 0 12px', color: sectionPrimaryColor }}>
            {job.position} at {job.company}
          </Title>
          <Paragraph style={{ marginBottom: '8px', color: '#666' }}>
            {job.startDate} - {job.endDate || 'Present'}
            {job.location && ` • ${job.location}`}
          </Paragraph>
          {job.description && (
            <Paragraph style={{ margin: 0 }}>
              {job.description}
            </Paragraph>
          )}
        </div>
      ))}
    </Card>
  );

  const renderProfessionalTemplate = () => (
    <Card key="jobs" style={{ 
      marginBottom: '32px',
      position: 'relative',
      borderRadius: '20px',
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
      
      {/* Professional Header */}
      <div style={{
        background: `linear-gradient(135deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}dd)`,
        padding: '32px',
        textAlign: 'center',
        borderRadius: '20px 20px 0 0',
        position: 'relative'
      }}>
        <Title level={2} style={{ color: 'white', margin: 0, fontSize: '28px', fontWeight: '700' }}>
          👔 Professional Showcase
        </Title>
        <Text style={{ color: 'rgba(255,255,255,0.9)', fontSize: '16px', marginTop: '8px' }}>
          Career Excellence Journey
        </Text>
      </div>
      
      <div style={{ padding: '32px' }}>
        {jobsData.map((job: any, index: number) => (
          <div key={index} style={{
            background: '#fff',
            borderRadius: '16px',
            padding: '28px',
            marginBottom: index < jobsData.length - 1 ? '24px' : 0,
            border: `1px solid ${sectionPrimaryColor}15`,
            position: 'relative',
            boxShadow: '0 8px 25px rgba(0,0,0,0.08)',
            overflow: 'hidden'
          }}>
            {/* Professional Badge */}
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
              {index === 0 ? 'Current' : `Position ${index + 1}`}
            </div>
            
            {/* Company Logo Placeholder */}
            <div style={{
              position: 'absolute',
              top: '20px',
              right: '20px',
              width: '60px',
              height: '60px',
              background: `linear-gradient(135deg, ${sectionPrimaryColor}20, ${sectionPrimaryColor}40)`,
              borderRadius: '12px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontSize: '24px'
            }}>
              🏢
            </div>
            
            <Row gutter={[24, 16]}>
              <Col xs={24} sm={16}>
                <div>
                  <Title level={3} style={{ 
                    color: '#2c3e50', 
                    marginBottom: '4px',
                    fontSize: '22px',
                    fontWeight: '600'
                  }}>
                    {job.position}
                  </Title>
                  
                  <Title level={4} style={{ 
                    color: sectionPrimaryColor, 
                    marginBottom: '12px',
                    fontSize: '18px',
                    fontWeight: '500'
                  }}>
                    {job.company}
                  </Title>
                  
                  {job.location && (
                    <div style={{
                      display: 'flex',
                      alignItems: 'center',
                      marginBottom: '16px'
                    }}>
                      <span style={{ fontSize: '14px', marginRight: '8px', color: sectionPrimaryColor }}>📍</span>
                      <Text style={{ fontSize: '14px', color: '#666' }}>{job.location}</Text>
                    </div>
                  )}
                  
                  <div style={{
                    background: `${sectionPrimaryColor}08`,
                    padding: '12px 16px',
                    borderRadius: '8px',
                    marginBottom: '16px',
                    borderLeft: `4px solid ${sectionPrimaryColor}`
                  }}>
                    <Text style={{ fontSize: '14px', color: sectionPrimaryColor, fontWeight: '600' }}>
                      {job.startDate} - {job.endDate || 'Present'}
                      {job.endDate && (
                        <span style={{ color: '#666', fontWeight: '400', marginLeft: '8px' }}>
                          ({Math.ceil(((new Date(job.endDate).getTime() - new Date(job.startDate).getTime()) / (1000 * 60 * 60 * 24 * 30))) || 1} months)
                        </span>
                      )}
                    </Text>
                  </div>
                  
                  {job.description && (
                    <Paragraph style={{ 
                      fontSize: '15px', 
                      color: '#666',
                      lineHeight: 1.7,
                      margin: 0
                    }}>
                      {job.description}
                    </Paragraph>
                  )}
                </div>
              </Col>
            </Row>
          </div>
        ))}
      </div>
    </Card>
  );

  const renderExperienceTemplate = () => (
    <Card key="jobs" style={{ 
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
        ⭐ Experience Cards
      </Title>
      
      <Row gutter={[24, 24]}>
        {jobsData.map((job: any, index: number) => (
          <Col key={index} xs={24} sm={12} lg={8}>
            <div style={{
              background: 'linear-gradient(135deg, #fff 0%, #f8f9fa 100%)',
              borderRadius: '20px',
              padding: '24px',
              height: '100%',
              border: `2px solid ${sectionPrimaryColor}15`,
              position: 'relative',
              overflow: 'hidden',
              boxShadow: '0 12px 30px rgba(0,0,0,0.1)',
              transition: 'transform 0.3s ease, box-shadow 0.3s ease'
            }}>
              {/* Experience Level Indicator */}
              <div style={{
                position: 'absolute',
                top: 0,
                left: 0,
                right: 0,
                height: '5px',
                background: `linear-gradient(90deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`
              }} />
              
              {/* Star Rating */}
              <div style={{
                position: 'absolute',
                top: '16px',
                right: '16px',
                display: 'flex',
                gap: '2px'
              }}>
                {[...Array(5)].map((_, starIndex) => (
                  <span key={starIndex} style={{
                    fontSize: '12px',
                    color: starIndex < (5 - index) ? sectionPrimaryColor : '#ddd'
                  }}>⭐</span>
                ))}
              </div>
              
              <div style={{ textAlign: 'center', marginBottom: '20px', marginTop: '20px' }}>
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
                  💼
                </div>
              </div>
              
              <Title level={4} style={{ 
                textAlign: 'center',
                color: '#2c3e50',
                marginBottom: '8px',
                fontSize: '16px',
                fontWeight: '600'
              }}>
                {job.position}
              </Title>
              
              <Text strong style={{ 
                display: 'block',
                textAlign: 'center',
                color: sectionPrimaryColor,
                marginBottom: '12px',
                fontSize: '14px'
              }}>
                {job.company}
              </Text>
              
              {job.location && (
                <Text style={{ 
                  display: 'block',
                  textAlign: 'center',
                  fontSize: '12px', 
                  color: '#666', 
                  marginBottom: '16px' 
                }}>
                  📍 {job.location}
                </Text>
              )}
              
              <div style={{
                background: `${sectionPrimaryColor}10`,
                padding: '8px 12px',
                borderRadius: '20px',
                textAlign: 'center',
                marginBottom: '16px'
              }}>
                <Text style={{ fontSize: '11px', color: sectionPrimaryColor, fontWeight: '600' }}>
                  {job.startDate} - {job.endDate || 'Present'}
                </Text>
              </div>
              
              {job.description && (
                <Paragraph style={{ 
                  fontSize: '12px', 
                  color: '#666',
                  lineHeight: 1.5,
                  margin: 0,
                  textAlign: 'center'
                }}>
                  {job.description.length > 80 ? job.description.substring(0, 80) + '...' : job.description}
                </Paragraph>
              )}
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  );

  const renderRoadmapTemplate = () => (
    <Card key="jobs" style={{ 
      marginBottom: '32px',
      position: 'relative',
      borderRadius: '16px',
      overflow: 'hidden'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor, textAlign: 'center', marginBottom: '32px' }}>
        🗺️ Career Roadmap
      </Title>
      
      <div style={{ padding: '20px', position: 'relative' }}>
        {/* Roadmap Path */}
        <div style={{
          position: 'absolute',
          top: '80px',
          left: '50%',
          transform: 'translateX(-50%)',
          width: '4px',
          height: `${(jobsData.length - 1) * 200 + 100}px`,
          background: `linear-gradient(to bottom, ${sectionPrimaryColor}, ${sectionPrimaryColor}40)`,
          borderRadius: '2px',
          zIndex: 1
        }} />
        
        {jobsData.map((job: any, index: number) => (
          <div key={index} style={{ 
            position: 'relative',
            marginBottom: '60px',
            zIndex: 2
          }}>
            {/* Roadmap Milestone */}
            <div style={{
              position: 'absolute',
              left: '50%',
              top: '20px',
              transform: 'translateX(-50%)',
              width: '24px',
              height: '24px',
              background: sectionPrimaryColor,
              borderRadius: '50%',
              border: '6px solid white',
              boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
              zIndex: 3
            }} />
            
            {/* Milestone Number */}
            <div style={{
              position: 'absolute',
              left: '50%',
              top: '-15px',
              transform: 'translateX(-50%)',
              width: '40px',
              height: '40px',
              background: `${sectionPrimaryColor}15`,
              borderRadius: '50%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              border: `2px solid ${sectionPrimaryColor}`,
              zIndex: 4
            }}>
              <Text style={{ fontSize: '12px', color: sectionPrimaryColor, fontWeight: '700' }}>
                {jobsData.length - index}
              </Text>
            </div>
            
            <Row gutter={[32, 24]}>
              <Col xs={24} md={12} style={{ order: index % 2 === 0 ? 1 : 2 }}>
                <div style={{
                  background: index % 2 === 0 ? '#fff' : `${sectionPrimaryColor}05`,
                  borderRadius: '16px',
                  padding: '24px',
                  border: `2px solid ${sectionPrimaryColor}20`,
                  position: 'relative',
                  boxShadow: '0 8px 25px rgba(0,0,0,0.08)',
                  marginTop: '40px'
                }}>
                  {/* Achievement Badge */}
                  <div style={{
                    position: 'absolute',
                    top: '-15px',
                    left: '20px',
                    background: index === 0 ? '#52c41a' : sectionPrimaryColor,
                    color: 'white',
                    padding: '6px 12px',
                    borderRadius: '12px',
                    fontSize: '10px',
                    fontWeight: '600'
                  }}>
                    {index === 0 ? '🎯 CURRENT' : '✅ COMPLETED'}
                  </div>
                  
                  <Title level={4} style={{ 
                    color: sectionPrimaryColor, 
                    marginBottom: '8px',
                    fontSize: '18px'
                  }}>
                    {job.position}
                  </Title>
                  
                  <Title level={5} style={{ 
                    color: '#2c3e50', 
                    marginBottom: '12px',
                    fontWeight: '600'
                  }}>
                    {job.company}
                  </Title>
                  
                  {job.location && (
                    <div style={{
                      display: 'flex',
                      alignItems: 'center',
                      marginBottom: '12px'
                    }}>
                      <span style={{ fontSize: '14px', marginRight: '8px' }}>📍</span>
                      <Text style={{ fontSize: '13px', color: '#666' }}>{job.location}</Text>
                    </div>
                  )}
                  
                  <div style={{
                    display: 'flex',
                    gap: '8px',
                    marginBottom: '12px'
                  }}>
                    <div style={{
                      background: `${sectionPrimaryColor}15`,
                      padding: '4px 8px',
                      borderRadius: '6px'
                    }}>
                      <Text style={{ fontSize: '10px', color: sectionPrimaryColor, fontWeight: '600' }}>
                        START: {job.startDate}
                      </Text>
                    </div>
                    <div style={{
                      background: job.endDate ? `${sectionPrimaryColor}25` : '#52c41a15',
                      padding: '4px 8px',
                      borderRadius: '6px'
                    }}>
                      <Text style={{ fontSize: '10px', color: job.endDate ? sectionPrimaryColor : '#52c41a', fontWeight: '600' }}>
                        {job.endDate ? `END: ${job.endDate}` : 'ONGOING'}
                      </Text>
                    </div>
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
              
              <Col xs={24} md={12} style={{ 
                order: index % 2 === 0 ? 2 : 1,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center'
              }}>
                <div style={{
                  textAlign: 'center',
                  padding: '20px'
                }}>
                  <div style={{
                    width: '100px',
                    height: '100px',
                    background: `linear-gradient(135deg, ${sectionPrimaryColor}20, ${sectionPrimaryColor}40)`,
                    borderRadius: '20px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '36px',
                    margin: '0 auto 12px',
                    transform: 'rotate(5deg)',
                    boxShadow: '0 12px 25px rgba(0,0,0,0.1)'
                  }}>
                    {index === 0 ? '🎯' : index === 1 ? '🚀' : '💼'}
                  </div>
                  <Text style={{ 
                    fontSize: '12px', 
                    color: '#666',
                    fontWeight: '500',
                    textTransform: 'uppercase',
                    letterSpacing: '1px'
                  }}>
                    Phase {jobsData.length - index}
                  </Text>
                </div>
              </Col>
            </Row>
          </div>
        ))}
      </div>
    </Card>
  );

  // Template selector
  switch (template) {
    case 'career':
      return renderCareerTemplate();
    case 'corporate':
      return renderCorporateTemplate();
    case 'professional':
      return renderProfessionalTemplate();
    case 'experience':
      return renderExperienceTemplate();
    case 'roadmap':
      return renderRoadmapTemplate();
    case 'timeline':
      return renderTimelineTemplate();
    default:
      return renderTimelineTemplate(); // fallback
  }
};

export default JobsSection; 