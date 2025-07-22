import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './CardsTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface CardsTemplateProps {
  projects: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const CardsTemplate: React.FC<CardsTemplateProps> = ({
  projects,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="projects" className="cards-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="cards-template__title" style={{ color: sectionPrimaryColor }}>
        🃏 Project Cards
      </Title>
       
      <Row gutter={[16, 16]}>
        {projects.map((project: any, index: number) => (
          <Col key={index} xs={24} sm={12} md={8}>
            <div 
              className="cards-template__project-card"
              style={{ 
                border: `1px solid ${sectionPrimaryColor}20`
              }}
            >
              <Text strong className="cards-template__project-name">{project.name}</Text>
              <br />
              {project.description && (
                <Paragraph className="cards-template__project-description">
                  {project.description}
                </Paragraph>
              )}
              {project.technologies && (
                <Text type="secondary" className="cards-template__project-tech">
                  Tech: {project.technologies}
                </Text>
              )}
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  );
};

export default CardsTemplate; 