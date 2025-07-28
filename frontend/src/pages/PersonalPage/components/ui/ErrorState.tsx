import React from 'react';
import { Alert } from 'antd';

interface ErrorStateProps {
  error: string | null;
  title?: string;
  type?: 'error' | 'warning';
}

/**
 * Reusable error state component
 * Follows Single Responsibility Principle - only handles error UI
 */
export const ErrorState: React.FC<ErrorStateProps> = ({ 
  error, 
  title = "Unable to load profile data",
  type = "error"
}) => {
  return (
    <div style={{ padding: '24px' }}>
      <Alert
        message={title}
        description={error || "Please check your connection and try again."}
        type={type}
        showIcon
      />
    </div>
  );
};
