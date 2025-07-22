import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './DetailedTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface DetailedTemplateProps {
  projects: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const DetailedTemplate: React.FC<DetailedTemplateProps> = ({
  projects,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="projects" className="detailed-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="detailed-template__title" style={{ color: sectionPrimaryColor }}>
        📋 Detailed View
      </Title>
       
      {projects.map((project: any, index: number) => (
        <div 
          key={index} 
          className="detailed-template__project-item"
          style={{ 
            border: `1px solid ${sectionPrimaryColor}20`
          }}
        >
          <Row gutter={[16, 16]}>
            <Col xs={24} md={16}>
              <Title level={4} className="detailed-template__project-name" style={{ color: sectionPrimaryColor }}>
                {project.name}
              </Title>
              {project.description && (
                <Paragraph className="detailed-template__project-description">
                  {project.description}
                </Paragraph>
              )}
              {project.technologies && (
                <Text type="secondary" className="detailed-template__project-tech">
                  Technologies: {project.technologies}
                </Text>
              )}
            </Col>
            <Col xs={24} md={8}>
              <div className="detailed-template__links-container">
                {project.demoUrl && (
                  <div className="detailed-template__demo-link">
                    <Text strong>🔗 Demo Available</Text>
                  </div>
                )}
                {project.sourceUrl && (
                  <div className="detailed-template__source-link">
                    <Text strong>💻 Source Code</Text>
                  </div>
                )}
              </div>
            </Col>
          </Row>
        </div>
      ))}
    </Card>
  );
};

export default DetailedTemplate; 