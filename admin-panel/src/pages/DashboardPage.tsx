import React from 'react';
import { Row, Col, Card, Statistic, Typography, Spin, Alert, Button, Progress, Badge } from 'antd';
import { 
  UserOutlined, 
  FileTextOutlined, 
  CommentOutlined, 
  LikeOutlined,
  ReloadOutlined,
  WarningOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  StarOutlined,
  BugOutlined,
  ThunderboltOutlined,
  DatabaseOutlined,
  HeartOutlined
} from '@ant-design/icons';
import { useDashboard } from '../hooks/useDashboard';

const { Title, Text } = Typography;

export const DashboardPage: React.FC = () => {
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

  const { userStats, contentStats, moderationStats, systemStats } = stats;

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
              value={userStats.totalUsers}
              prefix={<UserOutlined />}
              valueStyle={{ color: '#3f8600' }}
              suffix={
                <Badge 
                  count={`+${userStats.dailySignups}`} 
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
              value={userStats.activeUsers}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#1890ff' }}
              suffix={<Text type="secondary">({Math.round((userStats.activeUsers / userStats.totalUsers) * 100)}%)</Text>}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Online Now"
              value={userStats.onlineUsers}
              prefix={<ThunderboltOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Growth Rate"
              value={userStats.growthRate}
              prefix={<HeartOutlined />}
              suffix="%"
              valueStyle={{ color: userStats.growthRate >= 0 ? '#3f8600' : '#cf1322' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Content Statistics */}
      <Title level={4} style={{ marginBottom: 16 }}>Content Statistics</Title>
      <Row gutter={[16, 16]} style={{ marginBottom: 32 }}>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Total Blog Posts"
              value={contentStats.totalBlogPosts}
              prefix={<FileTextOutlined />}
              valueStyle={{ color: '#1890ff' }}
              suffix={
                <Badge 
                  count={`+${contentStats.blogPostsToday}`} 
                  style={{ backgroundColor: '#1890ff' }} 
                  title="New today"
                />
              }
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Published Posts"
              value={contentStats.publishedBlogPosts}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Draft Posts"
              value={contentStats.draftBlogPosts}
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <Card>
            <Statistic
              title="Average Rating"
              value={contentStats.averageRating}
              precision={1}
              prefix={<StarOutlined />}
              suffix="/ 5"
              valueStyle={{ color: '#eb2f96' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Comments Statistics */}
      <Row gutter={[16, 16]} style={{ marginBottom: 32 }}>
        <Col xs={24} sm={12}>
          <Card>
            <Statistic
              title="Total Comments"
              value={contentStats.totalComments}
              prefix={<CommentOutlined />}
              valueStyle={{ color: '#722ed1' }}
              suffix={
                <Badge 
                  count={`+${contentStats.commentsToday}`} 
                  style={{ backgroundColor: '#722ed1' }} 
                  title="New today"
                />
              }
            />
          </Card>
        </Col>
      </Row>

      {/* Moderation & System Overview */}
      <Row gutter={[16, 16]} style={{ marginBottom: 32 }}>
        <Col xs={24} lg={12}>
          <Card title="Moderation Overview" size="small">
            <div style={{ marginBottom: 16 }}>
              <Text strong>Pending Reports: </Text>
              <Badge count={moderationStats.pendingReports} style={{ backgroundColor: '#ff4d4f' }} />
              <Text type="secondary"> / {moderationStats.totalReports} total</Text>
            </div>
            <div style={{ marginBottom: 16 }}>
              <Text strong>Pending Flags: </Text>
              <Badge count={moderationStats.pendingFlags} style={{ backgroundColor: '#faad14' }} />
              <Text type="secondary"> / {moderationStats.totalFlags} total</Text>
            </div>
            <div style={{ marginBottom: 16 }}>
              <Text strong>Banned Users: </Text>
              <Badge count={moderationStats.bannedUsers} style={{ backgroundColor: '#ff7a45' }} />
            </div>
            <div>
              <Text strong>Deleted Content: </Text>
              <Text type="secondary">{moderationStats.deletedPosts} posts, {moderationStats.deletedComments} comments</Text>
            </div>
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="System Health" size="small">
            <div style={{ marginBottom: 16 }}>
              <Text strong>CPU Usage</Text>
              <Progress 
                percent={Math.round(systemStats.cpuUsage)} 
                status={systemStats.cpuUsage > 80 ? 'exception' : systemStats.cpuUsage > 60 ? 'active' : 'success'}
                size="small"
              />
            </div>
            <div style={{ marginBottom: 16 }}>
              <Text strong>Memory Usage</Text>
              <Progress 
                percent={Math.round(systemStats.memoryUsage)} 
                status={systemStats.memoryUsage > 80 ? 'exception' : systemStats.memoryUsage > 60 ? 'active' : 'success'}
                size="small"
              />
            </div>
            <div style={{ marginBottom: 16 }}>
              <Text strong>Disk Usage</Text>
              <Progress 
                percent={Math.round(systemStats.diskUsage)} 
                status={systemStats.diskUsage > 80 ? 'exception' : systemStats.diskUsage > 60 ? 'active' : 'success'}
                size="small"
              />
            </div>
            <div style={{ marginBottom: 8 }}>
              <Text strong>Avg Response Time: </Text>
              <Text type="secondary">{systemStats.averageResponseTime}ms</Text>
            </div>
            <div>
              <Text strong>Errors (Last Hour): </Text>
              <Badge 
                count={systemStats.errorsLastHour} 
                style={{ backgroundColor: systemStats.errorsLastHour > 0 ? '#ff4d4f' : '#52c41a' }} 
              />
            </div>
          </Card>
        </Col>
      </Row>
    </div>
  );
};