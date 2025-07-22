import React from 'react';
import { Card, Typography } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './ShowcaseTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface ShowcaseTemplateProps {
  projects: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const ShowcaseTemplate: React.FC<ShowcaseTemplateProps> = ({
  projects,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="projects" className="showcase-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="showcase-template__title" style={{ color: sectionPrimaryColor }}>
        🚀 Project Showcase
      </Title>
       
      {projects.map((project: any, index: number) => (
        <div 
          key={index} 
          className="showcase-template__project-item"
          style={{ 
            borderBottom: index < projects.length - 1 ? `1px solid ${sectionPrimaryColor}20` : 'none' 
          }}
        >
          <Text strong className="showcase-template__project-name">{project.name}</Text>
          <br />
          {project.description && (
            <Paragraph className="showcase-template__project-description">
              {project.description}
            </Paragraph>
          )}
          {project.technologies && (
            <Text type="secondary" className="showcase-template__project-tech">
              {project.technologies}
            </Text>
          )}
        </div>
      ))}
    </Card>
  );
};

export default ShowcaseTemplate; 