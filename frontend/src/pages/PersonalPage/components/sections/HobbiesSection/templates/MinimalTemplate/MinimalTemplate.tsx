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
    <Card key="hobbies" style={{ marginBottom: '24px', position: 'relative' }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor }}>✨ Simple Tags</Title>
       
      <div style={{ marginTop: '16px' }}>
        {hobbies.map((hobby: any, index: number) => (
          <span key={index}>
            <Text>{hobby.name}</Text>
            {index < hobbies.length - 1 && <Text type="secondary"> • </Text>}
          </span>
        ))}
      </div>
    </Card>
  );
};

export default MinimalTemplate; 