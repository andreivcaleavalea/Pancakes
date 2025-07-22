import React from 'react';
import { Card, Typography, Row, Col, Tag } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './AcademicTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AcademicTemplateProps {
  educationData: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const AcademicTemplate: React.FC<AcademicTemplateProps> = ({
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
};

export default AcademicTemplate; 