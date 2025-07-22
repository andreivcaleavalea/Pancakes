import React from 'react';
import { Card, Avatar, Typography } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './MinimalTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface MinimalTemplateProps {
  user: any;
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const MinimalTemplate: React.FC<MinimalTemplateProps> = ({
  user,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="personal" style={{ 
      marginBottom: '24px', 
      textAlign: 'center', 
      padding: '32px', 
      position: 'relative' 
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Avatar
        size={80}
        src={user.avatar ? `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${user.avatar}` : undefined}
        style={{ marginBottom: '16px', border: `3px solid ${sectionPrimaryColor}` }}
      />
      <Title level={2} style={{ color: sectionPrimaryColor, margin: '16px 0 8px' }}>
        {user.name}
      </Title>
      <Text type="secondary" style={{ fontSize: '16px' }}>{user.email}</Text>
      {user.bio && (
        <Paragraph style={{ marginTop: '16px', fontSize: '14px' }}>
          {user.bio}
        </Paragraph>
      )}
    </Card>
  );
};

export default MinimalTemplate; 