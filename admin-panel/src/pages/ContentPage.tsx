import React, { useState } from 'react';
import { 
  Typography, 
  Card, 
  Table, 
  Button, 
  Space, 
  Tag, 
  Tooltip, 
  Modal, 
  Form, 
  Alert,
  Select,
  Badge,
  Input,
  Descriptions
} from 'antd';
import { 
  CheckCircleOutlined, 
  CloseCircleOutlined,
  ReloadOutlined,
  EyeOutlined,
  CalendarOutlined,
  UserOutlined,
  FileTextOutlined,
  CommentOutlined
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useContentModeration } from '../hooks/useContentModeration';
import { ContentFlag } from '../services/api';

const { Title, Text } = Typography;
const { TextArea } = Input;
const { Option } = Select;

export const ContentPage: React.FC = () => {
  const { 
    flags, 
    loading, 
    error, 
    actionLoading, 
    currentPage, 
    pageSize, 
    statusFilter,
    contentTypeFilter,
    approveContent, 
    rejectContent, 
    filterByStatus,
    filterByContentType,
    changePage, 
    refresh 
  } = useContentModeration();

  const [reviewModalVisible, setReviewModalVisible] = useState(false);
  const [selectedFlag, setSelectedFlag] = useState<ContentFlag | null>(null);
  const [reviewAction, setReviewAction] = useState<'approve' | 'reject' | null>(null);
  const [reviewForm] = Form.useForm();

  const handleReviewFlag = (flag: ContentFlag, action: 'approve' | 'reject') => {
    setSelectedFlag(flag);
    setReviewAction(action);
    setReviewModalVisible(true);
    reviewForm.resetFields();
  };

  const onReviewSubmit = async (values: any) => {
    if (!selectedFlag || !reviewAction) return;
    
    const { reason } = values;
    
    if (reviewAction === 'approve') {
      await approveContent(selectedFlag.id, reason);
    } else {
      await rejectContent(selectedFlag.id, reason);
    }
    
    setReviewModalVisible(false);
    setSelectedFlag(null);
    setReviewAction(null);
    reviewForm.resetFields();
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getSeverityColor = (severity: number) => {
    if (severity >= 8) return '#ff4d4f'; // High severity - red
    if (severity >= 5) return '#faad14'; // Medium severity - orange
    return '#52c41a'; // Low severity - green
  };

  const getStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'pending': return 'orange';
      case 'approved': return 'green';
      case 'rejected': return 'red';
      case 'under_review': return 'blue';
      default: return 'default';
    }
  };

  const getContentTypeIcon = (contentType: string) => {
    switch (contentType?.toLowerCase()) {
      case 'blogpost': return <FileTextOutlined />;
      case 'comment': return <CommentOutlined />;
      default: return <FileTextOutlined />;
    }
  };

  const columns: ColumnsType<ContentFlag> = [
    {
      title: 'Content',
      key: 'content',
      render: (_, record) => (
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          {getContentTypeIcon(record.contentType)}
          <div>
            <div style={{ fontWeight: 500 }}>
              {record.contentType} #{record.contentId.slice(-8)}
            </div>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              Flag Type: {record.flagType}
            </Text>
          </div>
        </div>
      ),
    },
    {
      title: 'Severity',
      dataIndex: 'severity',
      key: 'severity',
      render: (severity) => (
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <div 
            style={{ 
              width: 12, 
              height: 12, 
              borderRadius: '50%', 
              backgroundColor: getSeverityColor(severity) 
            }} 
          />
          <Text strong style={{ color: getSeverityColor(severity) }}>
            {severity}/10
          </Text>
        </div>
      ),
      sorter: (a, b) => a.severity - b.severity,
    },
    {
      title: 'Source',
      key: 'source',
      render: (_, record) => (
        <div>
          {record.autoDetected ? (
            <Tag color="purple">Auto-Detected</Tag>
          ) : (
            <div>
              <Tag color="blue">User Report</Tag>
              {record.flaggedBy && (
                <div style={{ fontSize: '12px', marginTop: 4 }}>
                  <UserOutlined style={{ marginRight: 4 }} />
                  <Text type="secondary">{record.flaggedBy}</Text>
                </div>
              )}
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => (
        <Tag color={getStatusColor(status)}>
          {status.replace('_', ' ').toUpperCase()}
        </Tag>
      ),
    },
    {
      title: 'Flagged Date',
      key: 'createdAt',
      render: (_, record) => (
        <Tooltip title={formatDate(record.createdAt || new Date().toISOString())}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
            <CalendarOutlined />
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {formatDate(record.createdAt || new Date().toISOString())}
            </Text>
          </div>
        </Tooltip>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Space>
          <Tooltip title="View Details">
            <Button icon={<EyeOutlined />} size="small" />
          </Tooltip>
          {record.status === 'pending' && (
            <>
              <Tooltip title="Approve Content">
                <Button 
                  icon={<CheckCircleOutlined />} 
                  size="small" 
                  type="primary"
                  loading={actionLoading === record.id}
                  onClick={() => handleReviewFlag(record, 'approve')}
                >
                  Approve
                </Button>
              </Tooltip>
              <Tooltip title="Reject Content">
                <Button 
                  icon={<CloseCircleOutlined />} 
                  size="small" 
                  danger
                  loading={actionLoading === record.id}
                  onClick={() => handleReviewFlag(record, 'reject')}
                >
                  Reject
                </Button>
              </Tooltip>
            </>
          )}
        </Space>
      ),
    },
  ];

  if (error) {
    return (
      <div>
        <Alert
          message="Failed to load content flags"
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

  const pendingCount = flags?.data?.filter(flag => flag.status === 'pending').length || 0;

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
        <div>
          <Title level={2}>Content Moderation</Title>
          <Text type="secondary">
            {flags ? `${flags.totalCount} total flags` : 'Loading flags...'}
            {pendingCount > 0 && (
              <Badge 
                count={pendingCount} 
                style={{ backgroundColor: '#faad14', marginLeft: 16 }} 
                title="Pending reviews"
              />
            )}
          </Text>
        </div>
        <Button icon={<ReloadOutlined />} onClick={refresh} loading={loading}>
          Refresh
        </Button>
      </div>

      <Card>
        <div style={{ marginBottom: 16, display: 'flex', gap: 16, alignItems: 'center' }}>
          <div>
            <Text strong>Filter by Status:</Text>
            <Select 
              style={{ width: 150, marginLeft: 8 }} 
              value={statusFilter || undefined}
              placeholder="All statuses"
              allowClear
              onChange={filterByStatus}
            >
              <Option value="pending">Pending</Option>
              <Option value="approved">Approved</Option>
              <Option value="rejected">Rejected</Option>
              <Option value="under_review">Under Review</Option>
            </Select>
          </div>
          
          <div>
            <Text strong>Filter by Type:</Text>
            <Select 
              style={{ width: 150, marginLeft: 8 }} 
              value={contentTypeFilter || undefined}
              placeholder="All types"
              allowClear
              onChange={filterByContentType}
            >
              <Option value="blogpost">Blog Posts</Option>
              <Option value="comment">Comments</Option>
            </Select>
          </div>
        </div>

        <Table
          columns={columns}
          dataSource={flags?.data || []}
          rowKey="id"
          loading={loading}
          pagination={{
            current: currentPage,
            pageSize: pageSize,
            total: flags?.totalCount || 0,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} flags`,
            onChange: changePage,
            onShowSizeChange: changePage,
          }}
          scroll={{ x: 'max-content' }}
        />
      </Card>

      {/* Review Flag Modal */}
      <Modal
        title={`${reviewAction === 'approve' ? 'Approve' : 'Reject'} Content`}
        open={reviewModalVisible}
        onCancel={() => {
          setReviewModalVisible(false);
          setSelectedFlag(null);
          setReviewAction(null);
          reviewForm.resetFields();
        }}
        footer={null}
        width={600}
      >
        {selectedFlag && reviewAction && (
          <div>
            <Alert
              message={`${reviewAction === 'approve' ? 'Approve' : 'Reject'} Content Flag`}
              description={`You are about to ${reviewAction} this ${selectedFlag.contentType} flag. This action will affect the content's visibility.`}
              type={reviewAction === 'approve' ? 'success' : 'warning'}
              showIcon
              style={{ marginBottom: 16 }}
            />

            <Descriptions title="Flag Details" bordered size="small" style={{ marginBottom: 16 }}>
              <Descriptions.Item label="Content Type">{selectedFlag.contentType}</Descriptions.Item>
              <Descriptions.Item label="Content ID">{selectedFlag.contentId}</Descriptions.Item>
              <Descriptions.Item label="Flag Type">{selectedFlag.flagType}</Descriptions.Item>
              <Descriptions.Item label="Severity">{selectedFlag.severity}/10</Descriptions.Item>
              <Descriptions.Item label="Source">
                {selectedFlag.autoDetected ? 'Auto-Detected' : `User: ${selectedFlag.flaggedBy}`}
              </Descriptions.Item>
              <Descriptions.Item label="Status">{selectedFlag.status}</Descriptions.Item>
            </Descriptions>
            
            <Form form={reviewForm} layout="vertical" onFinish={onReviewSubmit}>
              <Form.Item
                name="reason"
                label={`Reason for ${reviewAction}`}
                rules={[{ required: true, message: `Please provide a reason for ${reviewAction}ing this content` }]}
              >
                <TextArea 
                  rows={4} 
                  placeholder={`Explain why you are ${reviewAction}ing this content...`}
                />
              </Form.Item>
              
              <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
                <Space>
                  <Button onClick={() => setReviewModalVisible(false)}>
                    Cancel
                  </Button>
                  <Button 
                    type="primary" 
                    danger={reviewAction === 'reject'}
                    htmlType="submit"
                  >
                    {reviewAction === 'approve' ? 'Approve Content' : 'Reject Content'}
                  </Button>
                </Space>
              </Form.Item>
            </Form>
          </div>
        )}
      </Modal>
    </div>
  );
};