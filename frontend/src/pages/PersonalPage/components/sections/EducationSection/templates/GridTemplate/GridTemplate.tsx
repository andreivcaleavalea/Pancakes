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
    <Card key="education" className="grid-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="grid-template__title" style={{ color: sectionPrimaryColor }}>
        📋 Grid Layout
      </Title>
      
      <Row gutter={[24, 24]}>
        {educationData.map((edu: any, index: number) => (
          <Col key={index} xs={24} sm={12} lg={8}>
            <div 
              className="grid-template__card"
              style={{
                border: `1px solid ${sectionPrimaryColor}20`
              }}
            >
              <div className="grid-template__icon-container">
                <div 
                  className="grid-template__icon"
                  style={{
                    background: `${sectionPrimaryColor}15`
                  }}
                >
                  🎓
                </div>
              </div>
              
              <Title level={5} className="grid-template__institution" style={{ color: sectionPrimaryColor }}>
                {edu.institution}
              </Title>
              
              <Text strong className="grid-template__degree">
                {edu.degree}
              </Text>
              
              <Text className="grid-template__specialization">
                {edu.specialization}
              </Text>
              
              <div className="grid-template__dates">
                <Tag color={sectionPrimaryColor}>
                  {edu.startDate} - {edu.endDate || 'Present'}
                </Tag>
              </div>
              
              {edu.description && (
                <Paragraph className="grid-template__description">
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