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
import './DashboardPage.css';

const { Title, Text } = Typography;

const DashboardPage: React.FC = () => {
  const { stats, loading, error, refresh } = useDashboard();

  if (loading) {
    return (
      <div className="loading-container">
        <Spin size="large" />
        <p>Loading dashboard...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="error-container">
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
      <div className="no-data-container">
        <Alert message="No dashboard data available" type="warning" showIcon />
      </div>
    );
  }

  const { userStats } = stats;

  return (
    <div className="dashboard-container">
      <div className="dashboard-header">
        <div>
          <Title level={2} className="dashboard-title">Dashboard</Title>
          <Text type="secondary">Welcome to the Pancakes Admin Dashboard</Text>
        </div>
        <Button 
          icon={<ReloadOutlined />} 
          onClick={refresh}
          className="refresh-button"
          size="large"
        >
          Refresh
        </Button>
      </div>
      
      {/* User Statistics */}
      <Title level={4} style={{ marginBottom: 16 }}>User Statistics</Title>
      <Row gutter={[16, 16]} style={{ marginBottom: 32 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card className="stats-card">
            <UserOutlined className="stats-icon" style={{ color: '#3f8600' }} />
            <div className="stats-value" style={{ color: '#3f8600' }}>
              {userStats?.totalUsers || 0}
            </div>
            <div className="stats-label">Total Users</div>
            <Badge 
              count={`+${userStats?.dailySignups || 0}`} 
              style={{ backgroundColor: '#52c41a', marginTop: 8 }} 
              title="New today"
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card className="stats-card">
            <CheckCircleOutlined className="stats-icon" style={{ color: '#1890ff' }} />
            <div className="stats-value" style={{ color: '#1890ff' }}>
              {userStats?.activeUsers || 0}
            </div>
            <div className="stats-label">
              Active Users ({userStats ? Math.round((userStats.activeUsers / userStats.totalUsers) * 100) : 0}%)
            </div>
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