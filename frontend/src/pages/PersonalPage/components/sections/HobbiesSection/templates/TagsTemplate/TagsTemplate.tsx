import React from 'react';
import { Card, Typography, Tag } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './TagsTemplate.scss';

const { Title } = Typography;

interface TagsTemplateProps {
  hobbies: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const TagsTemplate: React.FC<TagsTemplateProps> = ({
  hobbies,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="hobbies" style={{ marginBottom: '24px', position: 'relative' }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor, marginBottom: '20px' }}>
        🏷️ Tag Cloud
      </Title>
       
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
        {hobbies.map((hobby: any, index: number) => (
          <Tag 
            key={index} 
            color={sectionPrimaryColor}
            style={{ 
              fontSize: '14px',
              padding: '4px 12px',
              borderRadius: '16px'
            }}
          >
            {hobby.name}
          </Tag>
        ))}
      </div>
    </Card>
  );
};

export default TagsTemplate; 