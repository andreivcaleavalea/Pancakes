import React from 'react';
import { Typography, Card, Alert } from 'antd';
import { ClockCircleOutlined } from '@ant-design/icons';
import './SettingsPage.css';

const { Title } = Typography;

const SettingsPage: React.FC = () => {
  return (
    <div className="settings-page">
      <Title level={2} className="settings-page-title">Settings</Title>
      <Card className="settings-coming-soon-card">
        <Alert
          className="settings-coming-soon-alert"
          message="Feature Coming Soon"
          description="Will be implemented soon"
          type="info"
          icon={<ClockCircleOutlined />}
          showIcon
        />
      </Card>
    </div>
  );
};

export default SettingsPage;