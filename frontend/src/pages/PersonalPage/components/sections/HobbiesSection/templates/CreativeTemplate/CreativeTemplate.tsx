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
    <Card key="hobbies" style={{ marginBottom: '24px', position: 'relative' }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor }}>🎨 Creative Layout</Title>
       
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '12px', marginTop: '16px' }}>
        {hobbies.map((hobby: any, index: number) => (
          <div key={index} style={{
            background: `${sectionPrimaryColor}15`,
            padding: '8px 16px',
            borderRadius: '20px',
            border: `2px solid ${sectionPrimaryColor}30`
          }}>
            <Text style={{ color: sectionPrimaryColor, fontWeight: '500' }}>
              {hobby.name}
            </Text>
          </div>
        ))}
      </div>
    </Card>
  );
};

export default CreativeTemplate; 