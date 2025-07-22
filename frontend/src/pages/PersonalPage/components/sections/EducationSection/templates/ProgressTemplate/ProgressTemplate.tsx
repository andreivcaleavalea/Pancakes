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
    <Card key="education" className="progress-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="progress-template__title" style={{ color: sectionPrimaryColor }}>
        📊 Progress Timeline
      </Title>
      
      <div className="progress-template__container">
        <div className="progress-template__timeline-container">
          <div 
            className="progress-template__timeline-line"
            style={{
              background: `linear-gradient(to bottom, ${sectionPrimaryColor}, ${sectionPrimaryColor}40)`
            }}
          />
          
          {educationData.map((edu: any, index: number) => (
            <div key={index} className="progress-template__item">
              {/* Progress Dot */}
              <div 
                className="progress-template__progress-dot"
                style={{ background: sectionPrimaryColor }}
              />
              
              {/* Progress Indicator */}
              <div 
                className="progress-template__progress-indicator"
                style={{
                  background: `${sectionPrimaryColor}15`
                }}
              >
                <Text className="progress-template__percentage" style={{ color: sectionPrimaryColor }}>
                  {Math.round(((index + 1) / educationData.length) * 100)}%
                </Text>
              </div>
              
              <div 
                className="progress-template__card"
                style={{
                  border: `1px solid ${sectionPrimaryColor}20`
                }}
              >
                <Row gutter={[16, 12]}>
                  <Col xs={24} sm={18}>
                    <Title level={4} className="progress-template__institution" style={{ color: sectionPrimaryColor }}>
                      {edu.institution}
                    </Title>
                    <Text strong className="progress-template__degree">{edu.degree}</Text>
                    <br />
                    <Text className="progress-template__specialization">{edu.specialization}</Text>
                    
                    {edu.description && (
                      <Paragraph className="progress-template__description">
                        {edu.description}
                      </Paragraph>
                    )}
                  </Col>
                  
                  <Col xs={24} sm={6}>
                    <div className="progress-template__dates-container">
                      <div 
                        className="progress-template__date-badge progress-template__date-badge--start"
                        style={{
                          background: `${sectionPrimaryColor}10`
                        }}
                      >
                        <Text className="progress-template__date-text" style={{ color: sectionPrimaryColor }}>
                          {edu.startDate}
                        </Text>
                      </div>
                      <br />
                      <div 
                        className="progress-template__date-badge progress-template__date-badge--end"
                        style={{
                          background: `${sectionPrimaryColor}20`
                        }}
                      >
                        <Text className="progress-template__date-text" style={{ color: sectionPrimaryColor }}>
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