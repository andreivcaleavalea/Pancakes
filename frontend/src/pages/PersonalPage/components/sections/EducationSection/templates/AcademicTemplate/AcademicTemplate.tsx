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
    <Card key="education" className="academic-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="academic-template__title" style={{ color: sectionPrimaryColor }}>
        🎓 Academic Timeline
      </Title>
      
      <Row gutter={[24, 24]}>
        {educationData.map((edu: any, index: number) => (
          <Col key={index} xs={24} md={12}>
            <div 
              className="academic-template__card"
              style={{ border: `2px solid ${sectionPrimaryColor}15` }}
            >
              <div 
                className="academic-template__card-header"
                style={{ background: `linear-gradient(90deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)` }}
              />
              
              <div className="academic-template__icon-container">
                <div 
                  className="academic-template__icon"
                  style={{ background: `${sectionPrimaryColor}15` }}
                >
                  🎓
                </div>
              </div>
              
              <Title level={4} className="academic-template__institution" style={{ color: sectionPrimaryColor }}>
                {edu.institution}
              </Title>
              
              <div 
                className="academic-template__degree-card"
                style={{ border: `1px solid ${sectionPrimaryColor}15` }}
              >
                <Text strong className="academic-template__degree-title">
                  {edu.degree}
                </Text>
                <br />
                <Text className="academic-template__specialization" style={{ color: sectionPrimaryColor }}>
                  Specialization: {edu.specialization}
                </Text>
              </div>
              
              <div className="academic-template__dates">
                <Tag color={sectionPrimaryColor}>{edu.startDate}</Tag>
                <span className="academic-template__arrow">→</span>
                <Tag color={sectionPrimaryColor}>{edu.endDate || 'Present'}</Tag>
              </div>
              
              {edu.description && (
                <Paragraph className="academic-template__description">
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