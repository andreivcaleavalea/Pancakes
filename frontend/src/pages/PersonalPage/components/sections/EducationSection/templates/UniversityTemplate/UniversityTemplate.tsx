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
    <Card key="education" className="university-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      {/* University Header */}
      <div 
        className="university-template__header"
        style={{
          background: `linear-gradient(135deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}dd)`
        }}
      >
        <Title level={2} className="university-template__title">
          🏛️ University Style
        </Title>
        <Text className="university-template__subtitle">
          Academic Excellence Journey
        </Text>
      </div>
      
      <div className="university-template__content">
        {educationData.map((edu: any, index: number) => (
          <div 
            key={index} 
            className="university-template__item"
            style={{
              border: `1px solid ${sectionPrimaryColor}15`
            }}
          >
            {/* University Badge */}
            <div 
              className="university-template__badge"
              style={{
                background: sectionPrimaryColor
              }}
            >
              #{index + 1}
            </div>
            
            <Row gutter={[24, 16]}>
              <Col xs={24} sm={16}>
                <div>
                  <Title level={3} className="university-template__institution" style={{ color: sectionPrimaryColor }}>
                    {edu.institution}
                  </Title>
                  
                  <div 
                    className="university-template__degree-card"
                    style={{
                      background: `${sectionPrimaryColor}08`,
                      borderLeft: `4px solid ${sectionPrimaryColor}`
                    }}
                  >
                    <Text strong className="university-template__degree">
                      {edu.degree}
                    </Text>
                    <br />
                    <Text className="university-template__specialization" style={{ color: sectionPrimaryColor }}>
                      Major: {edu.specialization}
                    </Text>
                  </div>
                  
                  {edu.description && (
                    <Paragraph className="university-template__description">
                      {edu.description}
                    </Paragraph>
                  )}
                </div>
              </Col>
              
              <Col xs={24} sm={8}>
                <div className="university-template__icon-container">
                  <div 
                    className="university-template__icon"
                    style={{
                      background: `linear-gradient(135deg, ${sectionPrimaryColor}15, ${sectionPrimaryColor}25)`
                    }}
                  >
                    🎓
                  </div>
                  <div 
                    className="university-template__date-badge"
                    style={{
                      background: `${sectionPrimaryColor}10`
                    }}
                  >
                    <Text className="university-template__date-text" style={{ color: sectionPrimaryColor }}>
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