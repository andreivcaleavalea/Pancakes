import React from 'react';
import { Typography, Card, Alert } from 'antd';
import { ClockCircleOutlined } from '@ant-design/icons';

const { Title } = Typography;

const AnalyticsPage: React.FC = () => {
  return (
    <div>
      <Title level={2}>Analytics</Title>
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

export default AnalyticsPage;