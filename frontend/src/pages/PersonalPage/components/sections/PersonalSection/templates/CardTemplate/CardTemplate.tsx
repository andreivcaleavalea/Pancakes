import React from 'react';
import { Card, Avatar, Typography } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './CardTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface CardTemplateProps {
  user: any;
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const CardTemplate: React.FC<CardTemplateProps> = ({
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
      position: 'relative',
      borderRadius: '12px'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Avatar
        size={100}
        src={user.avatar ? `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${user.avatar}` : undefined}
        style={{ marginBottom: '20px', border: `3px solid ${sectionPrimaryColor}` }}
      />
      <Title level={2} style={{ color: sectionPrimaryColor, margin: '20px 0 8px' }}>
        {user.name}
      </Title>
      <Text type="secondary" style={{ fontSize: '16px', display: 'block', marginBottom: '12px' }}>
        {user.email}
      </Text>
      {user.phoneNumber && (
        <Text type="secondary" style={{ fontSize: '14px', display: 'block', marginBottom: '16px' }}>
          📞 {user.phoneNumber}
        </Text>
      )}
      {user.bio && (
        <Paragraph style={{ marginTop: '20px', fontSize: '14px' }}>
          {user.bio}
        </Paragraph>
      )}
    </Card>
  );
};

export default CardTemplate; 