import React from 'react';
import { Card, Typography, Row, Col, Tag } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './ExperienceTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface ExperienceTemplateProps {
  jobs: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const ExperienceTemplate: React.FC<ExperienceTemplateProps> = ({
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
        {jobs.map((job: any, index: number) => (
          <Col key={index} xs={24} md={12} lg={8}>
            <div style={{
              background: 'linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%)',
              borderRadius: '16px',
              padding: '24px',
              height: '100%',
              border: `2px solid ${sectionPrimaryColor}15`,
              position: 'relative',
              overflow: 'hidden',
              boxShadow: '0 8px 25px rgba(0,0,0,0.08)',
              transition: 'transform 0.3s ease, box-shadow 0.3s ease'
            }}>
              {/* Experience Level Indicator */}
              <div style={{
                position: 'absolute',
                top: 0,
                left: 0,
                width: '100%',
                height: '6px',
                background: `linear-gradient(90deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`
              }} />
              
              {/* Experience Badge */}
              <div style={{
                position: 'absolute',
                top: '12px',
                right: '12px',
                background: sectionPrimaryColor,
                color: 'white',
                padding: '4px 8px',
                borderRadius: '8px',
                fontSize: '10px',
                fontWeight: '600'
              }}>
                #{index + 1}
              </div>
              
              <div style={{ textAlign: 'center', marginBottom: '20px', marginTop: '16px' }}>
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
                  {index === 0 ? '🎯' : index === 1 ? '⭐' : '💼'}
                </div>
              </div>
              
              <Title level={4} style={{ 
                margin: 0, 
                color: sectionPrimaryColor,
                fontSize: '16px',
                textAlign: 'center',
                marginBottom: '8px'
              }}>
                {job.position}
              </Title>
              
              <Text strong style={{ 
                display: 'block',
                textAlign: 'center',
                fontSize: '14px',
                marginBottom: '12px'
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
              
              <div style={{ textAlign: 'center', marginBottom: '16px' }}>
                <Tag size="small" color={sectionPrimaryColor}>
                  {job.startDate} - {job.endDate || 'Present'}
                </Tag>
              </div>
              
              {job.description && (
                <Paragraph style={{ 
                  fontSize: '12px', 
                  color: '#666',
                  lineHeight: 1.4,
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
};

export default ExperienceTemplate; 