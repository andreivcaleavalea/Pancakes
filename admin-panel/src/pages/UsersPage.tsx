import React, { useState } from 'react';
import { 
  Typography, 
  Card, 
  Table, 
  Input, 
  Button, 
  Space, 
  Tag, 
  Avatar, 
  Tooltip, 
  Modal, 
  Form, 
  DatePicker, 
  Alert,
  Spin,
  Badge
} from 'antd';
import { 
  SearchOutlined, 
  UserOutlined, 
  StopOutlined, 
  CheckCircleOutlined,
  ReloadOutlined,
  EyeOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useUsers } from '../hooks/useUsers';
import { UserOverview } from '../services/api';

const { Title, Text } = Typography;
const { confirm } = Modal;
const { TextArea } = Input;

export const UsersPage: React.FC = () => {
  const { 
    users, 
    loading, 
    error, 
    actionLoading, 
    currentPage, 
    pageSize, 
    searchTerm, 
    banUser, 
    unbanUser, 
    search, 
    changePage, 
    refresh 
  } = useUsers();

  const [banModalVisible, setBanModalVisible] = useState(false);
  const [selectedUser, setSelectedUser] = useState<UserOverview | null>(null);
  const [banForm] = Form.useForm();

  const handleSearch = (value: string) => {
    search(value.trim());
  };

  const handleBanUser = (user: UserOverview) => {
    setSelectedUser(user);
    setBanModalVisible(true);
    banForm.resetFields();
  };

  const handleUnbanUser = (user: UserOverview) => {
    confirm({
      title: 'Unban User',
      icon: <ExclamationCircleOutlined />,
      content: (
        <div>
          <p>Are you sure you want to unban <strong>{user.name}</strong>?</p>
          <p>Email: {user.email}</p>
        </div>
      ),
      onOk: async () => {
        await unbanUser(user.id, 'Admin unban action');
      },
    });
  };

  const onBanSubmit = async (values: any) => {
    if (!selectedUser) return;
    
    const { reason, expiresAt } = values;
    const expirationDate = expiresAt ? expiresAt.toISOString() : undefined;
    
    await banUser(selectedUser.id, reason, expirationDate);
    setBanModalVisible(false);
    setSelectedUser(null);
    banForm.resetFields();
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

  const columns: ColumnsType<UserOverview> = [
    {
      title: 'User',
      key: 'user',
      render: (_, record) => (
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <Avatar icon={<UserOutlined />} />
          <div>
            <div style={{ fontWeight: 500 }}>{record.name}</div>
            <Text type="secondary" style={{ fontSize: '12px' }}>{record.email}</Text>
          </div>
        </div>
      ),
    },
    {
      title: 'Provider',
      dataIndex: 'provider',
      key: 'provider',
      render: (provider) => (
        <Tag color={provider === 'Google' ? 'blue' : provider === 'Local' ? 'green' : 'default'}>
          {provider}
        </Tag>
      ),
    },
    {
      title: 'Status',
      key: 'status',
      render: (_, record) => (
        <Space direction="vertical" size="small">
          <Tag color={record.isActive ? 'green' : 'red'}>
            {record.isActive ? 'Active' : 'Inactive'}
          </Tag>
          {record.isBanned && <Tag color="volcano">Banned</Tag>}
        </Space>
      ),
    },
    {
      title: 'Activity',
      key: 'activity',
      render: (_, record) => (
        <Space direction="vertical" size="small" style={{ fontSize: '12px' }}>
          <div>
            <Text strong>Posts:</Text> <Badge count={record.totalBlogPosts} style={{ backgroundColor: '#1890ff' }} />
          </div>
          <div>
            <Text strong>Comments:</Text> <Badge count={record.totalComments} style={{ backgroundColor: '#722ed1' }} />
          </div>
          {record.reportsCount > 0 && (
            <div>
              <Text strong>Reports:</Text> <Badge count={record.reportsCount} style={{ backgroundColor: '#ff4d4f' }} />
            </div>
          )}
        </Space>
      ),
    },
    {
      title: 'Joined',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date) => (
        <Tooltip title={formatDate(date)}>
          <Text type="secondary">{formatDate(date)}</Text>
        </Tooltip>
      ),
    },
    {
      title: 'Last Login',
      dataIndex: 'lastLoginAt',
      key: 'lastLoginAt',
      render: (date) => (
        <Tooltip title={date ? formatDate(date) : 'Never'}>
          <Text type="secondary">{date ? formatDate(date) : 'Never'}</Text>
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
          {record.isBanned ? (
            <Tooltip title="Unban User">
              <Button 
                icon={<CheckCircleOutlined />} 
                size="small" 
                type="primary"
                loading={actionLoading === record.id}
                onClick={() => handleUnbanUser(record)}
              >
                Unban
              </Button>
            </Tooltip>
          ) : (
            <Tooltip title="Ban User">
              <Button 
                icon={<StopOutlined />} 
                size="small" 
                danger
                loading={actionLoading === record.id}
                onClick={() => handleBanUser(record)}
              >
                Ban
              </Button>
            </Tooltip>
          )}
        </Space>
      ),
    },
  ];

  if (error) {
    return (
      <div>
        <Alert
          message="Failed to load users"
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

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
        <div>
          <Title level={2}>User Management</Title>
          <Text type="secondary">
            {users ? `${users.totalCount} total users` : 'Loading users...'}
          </Text>
        </div>
        <Button icon={<ReloadOutlined />} onClick={refresh} loading={loading}>
          Refresh
        </Button>
      </div>

      <Card>
        <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Input.Search
            placeholder="Search users by name or email..."
            allowClear
            enterButton={<SearchOutlined />}
            size="large"
            onSearch={handleSearch}
            defaultValue={searchTerm}
            style={{ maxWidth: 400 }}
          />
        </div>

        <Table
          columns={columns}
          dataSource={users?.data || []}
          rowKey="id"
          loading={loading}
          pagination={{
            current: currentPage,
            pageSize: pageSize,
            total: users?.totalCount || 0,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} users`,
            onChange: changePage,
            onShowSizeChange: changePage,
          }}
          scroll={{ x: 'max-content' }}
        />
      </Card>

      {/* Ban User Modal */}
      <Modal
        title={`Ban User: ${selectedUser?.name}`}
        open={banModalVisible}
        onCancel={() => {
          setBanModalVisible(false);
          setSelectedUser(null);
          banForm.resetFields();
        }}
        footer={null}
      >
        {selectedUser && (
          <div>
            <Alert
              message="Ban User"
              description={`You are about to ban ${selectedUser.name} (${selectedUser.email}). This action will prevent them from accessing the platform.`}
              type="warning"
              showIcon
              style={{ marginBottom: 16 }}
            />
            
            <Form form={banForm} layout="vertical" onFinish={onBanSubmit}>
              <Form.Item
                name="reason"
                label="Reason for ban"
                rules={[{ required: true, message: 'Please provide a reason for the ban' }]}
              >
                <TextArea 
                  rows={4} 
                  placeholder="Explain why this user is being banned..."
                />
              </Form.Item>
              
              <Form.Item
                name="expiresAt"
                label="Ban Expiration (Optional)"
                help="Leave empty for permanent ban"
              >
                <DatePicker 
                  showTime 
                  style={{ width: '100%' }}
                  placeholder="Select expiration date"
                  disabledDate={(current) => current && current.isBefore(new Date(), 'day')}
                />
              </Form.Item>
              
              <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
                <Space>
                  <Button onClick={() => setBanModalVisible(false)}>
                    Cancel
                  </Button>
                  <Button type="primary" danger htmlType="submit">
                    Ban User
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