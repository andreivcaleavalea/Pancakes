import React from 'react';
import { Card, Typography, Row, Col, Tag } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './GridTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface GridTemplateProps {
  educationData: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const GridTemplate: React.FC<GridTemplateProps> = ({
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
        📋 Grid Layout
      </Title>
      
      <Row gutter={[24, 24]}>
        {educationData.map((edu: any, index: number) => (
          <Col key={index} xs={24} sm={12} lg={8}>
            <div style={{
              background: '#fff',
              borderRadius: '12px',
              padding: '20px',
              height: '100%',
              border: `1px solid ${sectionPrimaryColor}20`,
              boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
              transition: 'transform 0.3s ease'
            }}>
              <div style={{ textAlign: 'center', marginBottom: '16px' }}>
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
                  🎓
                </div>
              </div>
              
              <Title level={5} style={{ 
                textAlign: 'center',
                color: sectionPrimaryColor,
                marginBottom: '8px',
                fontSize: '16px'
              }}>
                {edu.institution}
              </Title>
              
              <Text strong style={{ 
                display: 'block',
                textAlign: 'center',
                marginBottom: '4px',
                fontSize: '14px'
              }}>
                {edu.degree}
              </Text>
              
              <Text style={{ 
                display: 'block',
                textAlign: 'center',
                fontSize: '12px',
                color: '#666',
                marginBottom: '12px'
              }}>
                {edu.specialization}
              </Text>
              
              <div style={{ textAlign: 'center', marginBottom: '12px' }}>
                <Tag size="small" color={sectionPrimaryColor}>
                  {edu.startDate} - {edu.endDate || 'Present'}
                </Tag>
              </div>
              
              {edu.description && (
                <Paragraph style={{ 
                  fontSize: '12px', 
                  color: '#666',
                  lineHeight: 1.4,
                  margin: 0,
                  textAlign: 'center'
                }}>
                  {edu.description.length > 60 ? edu.description.substring(0, 60) + '...' : edu.description}
                </Paragraph>
              )}
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  );
};

export default GridTemplate; 