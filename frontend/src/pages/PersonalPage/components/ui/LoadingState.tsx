import React from 'react';
import { Spin, Typography } from 'antd';

const { Text } = Typography;

interface LoadingStateProps {
  message?: string;
}

/**
 * Reusable loading state component
 * Follows Single Responsibility Principle - only handles loading UI
 */
export const LoadingState: React.FC<LoadingStateProps> = ({ 
  message = "Loading your personal page..." 
}) => {
  return (
    <div style={{ padding: '24px', textAlign: 'center' }}>
      <Spin size="large" />
      <div style={{ marginTop: '16px' }}>
        <Text type="secondary">{message}</Text>
      </div>
    </div>
  );
};
