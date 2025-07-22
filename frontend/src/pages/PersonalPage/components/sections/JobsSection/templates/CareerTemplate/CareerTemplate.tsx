import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './CareerTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface CareerTemplateProps {
  jobs: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const CareerTemplate: React.FC<CareerTemplateProps> = ({
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
        🚀 Career Journey
      </Title>
      
      <div style={{ position: 'relative' }}>
        {/* Career timeline line */}
        <div style={{
          position: 'absolute',
          left: '24px',
          top: '20px',
          bottom: '20px',
          width: '2px',
          background: `linear-gradient(to bottom, ${sectionPrimaryColor}, ${sectionPrimaryColor}50)`,
          zIndex: 1
        }} />

        {jobs.map((job: any, index: number) => (
          <div key={index} style={{ 
            position: 'relative',
            marginBottom: '24px',
            paddingLeft: '60px'
          }}>
            {/* Career milestone */}
            <div style={{
              position: 'absolute',
              left: '16px',
              top: '16px',
              width: '16px',
              height: '16px',
              background: sectionPrimaryColor,
              borderRadius: '50%',
              border: '3px solid white',
              boxShadow: '0 2px 8px rgba(0,0,0,0.15)',
              zIndex: 2
            }} />

            <div style={{
              background: '#fff',
              borderRadius: '12px',
              padding: '20px',
              border: `1px solid ${sectionPrimaryColor}20`,
              boxShadow: '0 4px 12px rgba(0,0,0,0.05)'
            }}>
              <Row gutter={[16, 12]}>
                <Col xs={24} sm={18}>
                  <Title level={4} style={{ 
                    color: sectionPrimaryColor, 
                    marginBottom: '4px' 
                  }}>
                    {job.position}
                  </Title>
                  <Text strong style={{ fontSize: '16px' }}>{job.company}</Text>
                  <br />
                  {job.location && (
                    <>
                      <Text style={{ fontSize: '14px', color: '#666' }}>📍 {job.location}</Text>
                      <br />
                    </>
                  )}
                  <Text type="secondary" style={{ fontSize: '14px' }}>
                    {job.startDate} - {job.endDate || 'Present'}
                  </Text>
                  {job.description && (
                    <Paragraph style={{ 
                      marginTop: '8px', 
                      fontSize: '14px', 
                      color: '#666' 
                    }}>
                      {job.description}
                    </Paragraph>
                  )}
                </Col>
                <Col xs={24} sm={6}>
                  <div style={{ textAlign: 'center' }}>
                    <div style={{
                      background: `${sectionPrimaryColor}15`,
                      borderRadius: '8px',
                      width: '50px',
                      height: '50px',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      margin: '0 auto',
                      fontSize: '20px'
                    }}>
                      💼
                    </div>
                  </div>
                </Col>
              </Row>
            </div>
          </div>
        ))}
      </div>
    </Card>
  );
};

export default CareerTemplate; 