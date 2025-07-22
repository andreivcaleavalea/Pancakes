import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './UniversityTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface UniversityTemplateProps {
  educationData: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const UniversityTemplate: React.FC<UniversityTemplateProps> = ({
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
};

export default UniversityTemplate; 