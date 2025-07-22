import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './JourneyTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface JourneyTemplateProps {
  educationData: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const JourneyTemplate: React.FC<JourneyTemplateProps> = ({
  educationData,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
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
};

export default JourneyTemplate; 