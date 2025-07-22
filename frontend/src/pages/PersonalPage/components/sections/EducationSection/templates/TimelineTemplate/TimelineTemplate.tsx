import React from 'react';
import { Card, Typography } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './TimelineTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface TimelineTemplateProps {
  educationData: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const TimelineTemplate: React.FC<TimelineTemplateProps> = ({
  educationData,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="education" style={{ marginBottom: '24px', position: 'relative' }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor }}>🎓 Education</Title>
       
      {educationData.map((edu: any, index: number) => (
        <div key={index} style={{ 
          padding: '16px 0', 
          borderBottom: index < educationData.length - 1 ? `1px solid ${sectionPrimaryColor}20` : 'none' 
        }}>
          <Text strong style={{ fontSize: '16px' }}>{edu.institution}</Text>
          <br />
          <Text>{edu.degree} in {edu.specialization}</Text>
          <br />
          <Text type="secondary">{edu.startDate} - {edu.endDate || 'Present'}</Text>
          {edu.description && (
            <Paragraph style={{ marginTop: '8px', fontSize: '14px', color: '#666' }}>
              {edu.description}
            </Paragraph>
          )}
        </div>
      ))}
    </Card>
  );
};

export default TimelineTemplate; 