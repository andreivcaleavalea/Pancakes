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
    <Card key="education" className="journey-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="journey-template__title" style={{ color: sectionPrimaryColor }}>
        🛤️ Education Journey
      </Title>
      
      <div className="journey-template__container">
        {educationData.map((edu: any, index: number) => (
          <div key={index} className="journey-template__item">
            {/* Journey Line */}
            {index < educationData.length - 1 && (
              <div 
                className="journey-template__line"
                style={{
                  background: `linear-gradient(to bottom, ${sectionPrimaryColor}, ${sectionPrimaryColor}50)`
                }} 
              />
            )}
            
            <Row align="middle" gutter={[24, 24]}>
              <Col xs={24} md={12} style={{ order: index % 2 === 0 ? 1 : 2 }}>
                <div 
                  className="journey-template__card"
                  style={{
                    border: `2px solid ${sectionPrimaryColor}20`
                  }}
                >
                  <div 
                    className="journey-template__date-badge"
                    style={{
                      background: sectionPrimaryColor
                    }}
                  >
                    {edu.startDate} - {edu.endDate || 'Present'}
                  </div>
                  
                  <Title level={4} className="journey-template__institution" style={{ color: sectionPrimaryColor }}>
                    {edu.institution}
                  </Title>
                  <Text strong className="journey-template__degree">{edu.degree}</Text>
                  <br />
                  <Text className="journey-template__specialization">{edu.specialization}</Text>
                  
                  {edu.description && (
                    <Paragraph className="journey-template__description">
                      {edu.description}
                    </Paragraph>
                  )}
                </div>
              </Col>
              
              <Col xs={24} md={12} className="journey-template__icon-col" style={{ order: index % 2 === 0 ? 2 : 1 }}>
                <div 
                  className="journey-template__icon"
                  style={{
                    background: `linear-gradient(45deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`
                  }}
                >
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