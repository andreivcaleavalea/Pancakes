import React from 'react';
import { Row, Col, Card, Statistic, Typography, Spin, Alert, Button, Badge } from "antd";
import { 
  UserOutlined, 
  ReloadOutlined,
  CheckCircleOutlined,
  ThunderboltOutlined,
  HeartOutlined,
  ClockCircleOutlined
} from '@ant-design/icons';
import { useDashboard } from '../hooks/useDashboard';

const { Title, Text } = Typography;

const DashboardPage: React.FC = () => {
  const { stats, loading, error, refresh } = useDashboard();

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
        <p style={{ marginTop: 16 }}>Loading dashboard...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div>
        <Alert
          message="Failed to load dashboard data"
          description={error}
          type="error"
          showIcon
          action={
            <Button size="small" icon={<ReloadOutlined />} onClick={refresh}>
              Retry
            </Button>
          }
        />
      </div>
    );
  }

  if (!stats) {
    return (
      <div>
        <Alert message="No dashboard data available" type="warning" showIcon />
      </div>
    );
  }

  const { userStats } = stats;

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
        <div>
          <Title level={2}>Dashboard</Title>
          <Text type="secondary">Welcome to the Pancakes Admin Dashboard</Text>
        </div>
        <Button icon={<ReloadOutlined />} onClick={refresh}>
          Refresh
        </Button>
      </div>
      
      {/* User Statistics */}
      <Title level={4} style={{ marginBottom: 16 }}>User Statistics</Title>
      <Row gutter={[16, 16]} style={{ marginBottom: 32 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Users"
              value={userStats?.totalUsers || 0}
              prefix={<UserOutlined />}
              valueStyle={{ color: '#3f8600' }}
              suffix={
                <Badge 
                  count={`+${userStats?.dailySignups || 0}`} 
                  style={{ backgroundColor: '#52c41a' }} 
                  title="New today"
                />
              }
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Active Users"
              value={userStats?.activeUsers || 0}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#1890ff' }}
              suffix={<Text type="secondary">({userStats ? Math.round((userStats.activeUsers / userStats.totalUsers) * 100) : 0}%)</Text>}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Online Now"
              value={userStats?.onlineUsers || 0}
              prefix={<ThunderboltOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Growth Rate"
              value={userStats?.growthRate || 0}
              prefix={<HeartOutlined />}
              suffix="%"
              valueStyle={{ color: (userStats?.growthRate || 0) >= 0 ? '#3f8600' : '#cf1322' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Other Features - Coming Soon */}
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12}>
          <Card>
            <Alert
              message="Content Analytics"
              description="Will be implemented soon"
              type="info"
              icon={<ClockCircleOutlined />}
              showIcon
            />
          </Card>
        </Col>
        <Col xs={24} sm={12}>
          <Card>
            <Alert
              message="System Monitoring"
              description="Will be implemented soon"
              type="info"
              icon={<ClockCircleOutlined />}
              showIcon
            />
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default DashboardPage;