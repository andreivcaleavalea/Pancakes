import React from 'react';
import { Card, Button, Space, Typography } from 'antd';
import { SaveOutlined, UndoOutlined } from '@ant-design/icons';

const { Text } = Typography;

interface PersonalPageActionsProps {
  hasUnsavedChanges: boolean;
  isSaving: boolean;
  onSave: () => void;
  onRevert: () => void;
}

/**
 * Action buttons component for save/revert operations
 * Follows Single Responsibility Principle - only handles action UI
 */
export const PersonalPageActions: React.FC<PersonalPageActionsProps> = ({
  hasUnsavedChanges,
  isSaving,
  onSave,
  onRevert,
}) => {
  return (
    <Card 
      style={{ 
        marginTop: '32px', 
        textAlign: 'center', 
        background: hasUnsavedChanges ? '#fff7e6' : '#fafafa', 
        borderColor: hasUnsavedChanges ? '#ffa940' : '#d9d9d9' 
      }}
    >
      {hasUnsavedChanges && (
        <Text type="warning" style={{ display: 'block', marginBottom: '16px', fontWeight: 500 }}>
          ⚠️ You have unsaved changes
        </Text>
      )}
      
      <Space size="large">
        <Button 
          type="primary" 
          icon={<SaveOutlined />}
          size="large"
          loading={isSaving}
          disabled={!hasUnsavedChanges}
          onClick={onSave}
        >
          Save Changes
        </Button>
        <Button 
          icon={<UndoOutlined />}
          size="large"
          disabled={!hasUnsavedChanges || isSaving}
          onClick={onRevert}
        >
          Revert Changes
        </Button>
      </Space>
      
      {!hasUnsavedChanges && (
        <Text type="secondary" style={{ display: 'block', marginTop: '8px', fontSize: '12px' }}>
          All changes saved ✓
        </Text>
      )}
    </Card>
  );
};
