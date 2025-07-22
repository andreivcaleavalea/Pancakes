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
    <Card key="projects" style={{ marginBottom: '24px', position: 'relative' }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor }}>📋 Detailed View</Title>
       
      {projects.map((project: any, index: number) => (
        <div key={index} style={{ 
          padding: '20px',
          marginBottom: '16px',
          border: `1px solid ${sectionPrimaryColor}20`,
          borderRadius: '8px'
        }}>
          <Row gutter={[16, 16]}>
            <Col xs={24} md={16}>
              <Title level={4} style={{ color: sectionPrimaryColor }}>{project.name}</Title>
              {project.description && (
                <Paragraph>{project.description}</Paragraph>
              )}
              {project.technologies && (
                <Text type="secondary">Technologies: {project.technologies}</Text>
              )}
            </Col>
            <Col xs={24} md={8}>
              {project.demoUrl && (
                <div style={{ marginBottom: '8px' }}>
                  <Text strong>🔗 Demo Available</Text>
                </div>
              )}
              {project.sourceUrl && (
                <div>
                  <Text strong>💻 Source Code</Text>
                </div>
              )}
            </Col>
          </Row>
        </div>
      ))}
    </Card>
  );
};

export default DetailedTemplate; 