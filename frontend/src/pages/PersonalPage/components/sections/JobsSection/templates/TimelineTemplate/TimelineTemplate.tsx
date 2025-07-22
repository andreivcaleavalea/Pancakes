import React from 'react';
import { Card, Typography } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './TimelineTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface TimelineTemplateProps {
  jobs: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const TimelineTemplate: React.FC<TimelineTemplateProps> = ({
  jobs,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="jobs" style={{ marginBottom: '24px', position: 'relative' }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor }}>💼 Work Experience</Title>
       
      {jobs.map((job: any, index: number) => (
        <div key={index} style={{ 
          padding: '16px 0', 
          borderBottom: index < jobs.length - 1 ? `1px solid ${sectionPrimaryColor}20` : 'none' 
        }}>
          <Text strong style={{ fontSize: '16px' }}>{job.position}</Text>
          <br />
          <Text>{job.company}</Text>
          {job.location && (
            <>
              <br />
              <Text style={{ fontSize: '14px', color: '#666' }}>📍 {job.location}</Text>
            </>
          )}
          <br />
          <Text type="secondary">{job.startDate} - {job.endDate || 'Present'}</Text>
          {job.description && (
            <Paragraph style={{ marginTop: '8px', fontSize: '14px', color: '#666' }}>
              {job.description}
            </Paragraph>
          )}
        </div>
      ))}
    </Card>
  );
};

export default TimelineTemplate; 