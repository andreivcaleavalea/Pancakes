import React from 'react';
import { Typography, Card, Alert } from 'antd';
import { ClockCircleOutlined } from '@ant-design/icons';

const { Title } = Typography;

const SettingsPage: React.FC = () => {
  return (
    <div>
      <Title level={2}>Settings</Title>
      <Card>
        <Alert
          message="Feature Coming Soon"
          description="Will be implemented soon"
          type="info"
          icon={<ClockCircleOutlined />}
          showIcon
          style={{ textAlign: 'center' }}
        />
      </Card>
    </div>
  );
};

export default SettingsPage;