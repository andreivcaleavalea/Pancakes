import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './ProgressTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface ProgressTemplateProps {
  educationData: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const ProgressTemplate: React.FC<ProgressTemplateProps> = ({
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
};

export default ProgressTemplate; 