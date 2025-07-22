import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './RoadmapTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface RoadmapTemplateProps {
  jobs: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const RoadmapTemplate: React.FC<RoadmapTemplateProps> = ({
  jobs,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
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
          height: `${(jobs.length - 1) * 200 + 100}px`,
          background: `linear-gradient(to bottom, ${sectionPrimaryColor}, ${sectionPrimaryColor}40)`,
          borderRadius: '2px',
          zIndex: 1
        }} />
        
        {jobs.map((job: any, index: number) => (
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
                {jobs.length - index}
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
                    Phase {jobs.length - index}
                  </Text>
                </div>
              </Col>
            </Row>
          </div>
        ))}
      </div>
    </Card>
  );
};

export default RoadmapTemplate; 