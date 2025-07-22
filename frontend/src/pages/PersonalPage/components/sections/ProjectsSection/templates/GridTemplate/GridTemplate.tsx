import React from 'react';
import { Card, Typography, Row, Col, Tag } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './GridTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface GridTemplateProps {
  projects: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const GridTemplate: React.FC<GridTemplateProps> = ({
  projects,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="projects" className="grid-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="grid-template__title" style={{ color: sectionPrimaryColor }}>
        🎨 Creative Grid
      </Title>
      
      <Row gutter={[20, 20]}>
        {projects.map((project: any, index: number) => (
          <Col key={index} xs={24} sm={12} md={8} lg={6}>
            <div 
              className="grid-template__project-card"
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
                  🚀
                </div>
              </div>
              
              <Title level={5} className="grid-template__project-title" style={{ color: sectionPrimaryColor }}>
                {project.name}
              </Title>
              
              {project.description && (
                <Paragraph className="grid-template__project-description">
                  {project.description.length > 50 ? project.description.substring(0, 50) + '...' : project.description}
                </Paragraph>
              )}
              
              {project.technologies && (
                <div className="grid-template__tech-container">
                  <Tag color={sectionPrimaryColor} className="grid-template__tech-tag">
                    {project.technologies.split(',')[0]?.trim()}
                  </Tag>
                </div>
              )}
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  );
};

export default GridTemplate; 