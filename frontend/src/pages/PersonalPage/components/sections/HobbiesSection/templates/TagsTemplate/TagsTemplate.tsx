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
    <Card key="hobbies" className="tags-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="tags-template__title" style={{ color: sectionPrimaryColor }}>
        🏷️ Tag Cloud
      </Title>
       
      <div className="tags-template__container">
        {hobbies.map((hobby: any, index: number) => (
          <Tag 
            key={index} 
            color={sectionPrimaryColor}
            className="tags-template__tag"
          >
            {hobby.name}
          </Tag>
        ))}
      </div>
    </Card>
  );
};

export default TagsTemplate; 