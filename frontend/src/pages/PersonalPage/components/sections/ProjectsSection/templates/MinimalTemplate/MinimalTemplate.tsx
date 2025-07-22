import React from 'react';
import { Card, Typography } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './MinimalTemplate.scss';

const { Title, Text } = Typography;

interface MinimalTemplateProps {
  projects: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const MinimalTemplate: React.FC<MinimalTemplateProps> = ({
  projects,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="projects" className="minimal-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="minimal-template__title" style={{ color: sectionPrimaryColor }}>
        📝 Simple List
      </Title>
       
      {projects.map((project: any, index: number) => (
        <div key={index} className="minimal-template__project-item">
          <Text strong className="minimal-template__project-name">{project.name}</Text>
          {project.technologies && (
            <Text type="secondary" className="minimal-template__project-tech">
              ({project.technologies.split(',')[0]?.trim()})
            </Text>
          )}
        </div>
      ))}
    </Card>
  );
};

export default MinimalTemplate; 