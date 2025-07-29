import React from 'react';
import { Card, Avatar, Typography } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { useAdvancedStyles } from '../../../../../hooks/useAdvancedStyles';
import type { PersonalTemplateProps } from '../../../../../types';
import './CardTemplate.scss';

const { Title, Text, Paragraph } = Typography;

const CardTemplate: React.FC<PersonalTemplateProps> = ({
  user,
  sectionKey,

  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
  advancedSettings,
  editMode = true,
}) => {
  // Use shared hook instead of duplicated code
  const { cardStyles, cardClassName, getContentStyles, getTypographyStyles } = useAdvancedStyles(
    advancedSettings,
    'card-template',
    {
      textAlign: 'center' as const,
      padding: '32px',
      borderRadius: '12px',
    }
  );

  return (
    <Card 
      key="personal" 
      className={cardClassName}
      style={cardStyles}
    >
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
        editMode={editMode}
      />
      
      <div style={getContentStyles()}>
        <Avatar
          size={120}
          src={user.avatar ? `${import.meta.env.VITE_USER_API_URL || 'http://localhost:5141'}/${user.avatar}` : undefined}
          className="card-template__avatar"
        />
        <Title level={2} style={getTypographyStyles()}>
          {user.name}
        </Title>
        <Text type="secondary" style={getTypographyStyles()}>
          {user.email}
        </Text>
        {user.phoneNumber && (
          <Text style={getTypographyStyles()}>
            📞 {user.phoneNumber}
          </Text>
        )}
        {user.bio && (
          <Paragraph style={getTypographyStyles()}>
            {user.bio}
          </Paragraph>
        )}
      </div>
    </Card>
  );
};

export default CardTemplate; 