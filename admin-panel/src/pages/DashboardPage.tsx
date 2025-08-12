import React from 'react';
import { Row, Col, Card, Typography, Spin, Alert, Button, Badge } from "antd";
import { 
  UserOutlined, 
  ReloadOutlined,
  FileTextOutlined,
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

  const { userStats, contentStats } = stats;

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
      
      {/* Statistics */}
      <Title level={4} style={{ marginBottom: 16 }}>Statistics</Title>
      <Row gutter={[16, 16]} style={{ marginBottom: 32 }}>
        <Col xs={24} sm={12} lg={12}>
          <Card className="stats-card">
            <UserOutlined className="stats-icon" style={{ color: '#3f8600' }} />
            <div className="stats-value" style={{ color: '#3f8600' }}>
              {loading ? '...' : (userStats?.totalUsers?.toLocaleString() || '0')}
            </div>
            <div className="stats-label">Total Users</div>
            {!loading && (
              <Badge 
                count={userStats?.dailySignups ? `+${userStats.dailySignups}` : '0'} 
                style={{ backgroundColor: '#52c41a', marginTop: 8 }} 
                title="New since last login"
              />
            )}
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={12}>
          <Card className="stats-card">
            <FileTextOutlined className="stats-icon" style={{ color: '#1890ff' }} />
            <div className="stats-value" style={{ color: '#1890ff' }}>
              {loading ? '...' : (contentStats?.totalBlogPosts?.toLocaleString() || '0')}
            </div>
            <div className="stats-label">Total Posts</div>
            {!loading && (
              <Badge 
                count={contentStats?.blogPostsToday ? `+${contentStats.blogPostsToday}` : '0'} 
                style={{ backgroundColor: '#1890ff', marginTop: 8 }} 
                title="New since last login"
              />
            )}
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