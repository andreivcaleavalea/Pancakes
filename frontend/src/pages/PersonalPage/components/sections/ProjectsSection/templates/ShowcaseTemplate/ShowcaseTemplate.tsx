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
    <Card key="projects" style={{ marginBottom: '24px', position: 'relative' }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor }}>🚀 Project Showcase</Title>
       
      {projects.map((project: any, index: number) => (
        <div key={index} style={{ 
          padding: '16px 0', 
          borderBottom: index < projects.length - 1 ? `1px solid ${sectionPrimaryColor}20` : 'none' 
        }}>
          <Text strong style={{ fontSize: '16px' }}>{project.name}</Text>
          <br />
          {project.description && (
            <Paragraph style={{ marginTop: '8px', fontSize: '14px', color: '#666' }}>
              {project.description}
            </Paragraph>
          )}
          {project.technologies && (
            <Text type="secondary">{project.technologies}</Text>
          )}
        </div>
      ))}
    </Card>
  );
};

export default ShowcaseTemplate; 