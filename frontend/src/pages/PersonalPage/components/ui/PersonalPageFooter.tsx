import React from 'react';
import { Card, Typography } from 'antd';

const { Text } = Typography;

interface PersonalPageFooterProps {
  colorScheme: string;
  visibleSectionCount: number;
}

/**
 * Footer component showing page settings summary
 * Follows Single Responsibility Principle - only handles footer UI
 */
export const PersonalPageFooter: React.FC<PersonalPageFooterProps> = ({
  colorScheme,
  visibleSectionCount,
}) => {
  return (
    <Card style={{ marginTop: '16px', background: '#fafafa' }}>
      <Text type="secondary" style={{ fontSize: '12px' }}>
        <strong>Page Settings:</strong> Color scheme: {colorScheme} • 
        Visible sections: {visibleSectionCount}/5
      </Text>
    </Card>
  );
};
