import React from 'react';
import { Card, Typography } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './CreativeTemplate.scss';

const { Title, Text } = Typography;

interface CreativeTemplateProps {
  hobbies: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const CreativeTemplate: React.FC<CreativeTemplateProps> = ({
  hobbies,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="hobbies" className="creative-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="creative-template__title" style={{ color: sectionPrimaryColor }}>
        🎨 Creative Layout
      </Title>
       
      <div className="creative-template__container">
        {hobbies.map((hobby: any, index: number) => (
          <div 
            key={index} 
            className="creative-template__hobby-card"
            style={{
              background: `${sectionPrimaryColor}15`,
              border: `2px solid ${sectionPrimaryColor}30`
            }}
          >
            <Text className="creative-template__hobby-name" style={{ color: sectionPrimaryColor }}>
              {hobby.name}
            </Text>
          </div>
        ))}
      </div>
    </Card>
  );
};

export default CreativeTemplate; 