import React from 'react';
import { Card, Typography } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './MinimalTemplate.scss';

const { Title, Text } = Typography;

interface MinimalTemplateProps {
  hobbies: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const MinimalTemplate: React.FC<MinimalTemplateProps> = ({
  hobbies,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="hobbies" className="minimal-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="minimal-template__title" style={{ color: sectionPrimaryColor }}>
        ✨ Simple Tags
      </Title>
       
      <div className="minimal-template__container">
        {hobbies.map((hobby: any, index: number) => (
          <span key={index}>
            <Text className="minimal-template__hobby-name">{hobby.name}</Text>
            {index < hobbies.length - 1 && <Text type="secondary" className="minimal-template__separator"> • </Text>}
          </span>
        ))}
      </div>
    </Card>
  );
};

export default MinimalTemplate; 