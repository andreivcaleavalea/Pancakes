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
    <Card key="projects" style={{ marginBottom: '24px', position: 'relative' }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor }}>📝 Simple List</Title>
       
      {projects.map((project: any, index: number) => (
        <div key={index} style={{ marginBottom: '8px' }}>
          <Text strong>{project.name}</Text>
          {project.technologies && (
            <Text type="secondary" style={{ marginLeft: '8px' }}>
              ({project.technologies.split(',')[0]?.trim()})
            </Text>
          )}
        </div>
      ))}
    </Card>
  );
};

export default MinimalTemplate; 