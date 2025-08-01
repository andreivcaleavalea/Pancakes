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
  Badge
} from 'antd';
import { 
  SearchOutlined, 
  UserOutlined, 
  StopOutlined, 
  CheckCircleOutlined,
  ReloadOutlined,
  EyeOutlined,
  ExclamationCircleOutlined,
  InfoCircleOutlined
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useUsers } from '../hooks/useUsers';
import { UserOverview } from '../services/api';

const { Title, Text } = Typography;
const { confirm } = Modal;
const { TextArea } = Input;

const UsersPage: React.FC = () => {
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
          <p><strong>Email:</strong> {user.email}</p>
          {user.currentBanReason && <p><strong>Ban Reason:</strong> {user.currentBanReason}</p>}
          {user.currentBannedAt && <p><strong>Banned on:</strong> {formatDate(user.currentBannedAt)}</p>}
          {user.currentBanExpiresAt ? (
            <p><strong>Expires:</strong> {formatDate(user.currentBanExpiresAt)}</p>
          ) : (
            <p><strong>Type:</strong> Permanent ban</p>
          )}
          {user.totalBansCount > 1 && <p><strong>Previous bans:</strong> {user.totalBansCount - 1}</p>}
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
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      timeZoneName: 'short' 
    });
  };

  const getBanStatusText = (user: UserOverview) => {
    if (!user.isBanned) return null;
    
    if (user.currentBanExpiresAt && user.currentBanExpiresAt.trim() !== '') {
      const expirationDate = new Date(user.currentBanExpiresAt);
      const now = new Date();
      if (!isNaN(expirationDate.getTime()) && expirationDate > now) {
        return `Expires ${formatDate(user.currentBanExpiresAt)}`;
      } else if (!isNaN(expirationDate.getTime())) {
        return 'Ban expired (needs review)';
      }
    }
    return 'Permanent ban';
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
        <div>
          {record.isBanned ? (
            <Tooltip 
              title={
                <div>
                  <div><strong>Reason:</strong> {record.currentBanReason || 'No reason provided'}</div>
                  {record.currentBannedAt && <div><strong>Banned on:</strong> {formatDate(record.currentBannedAt)}</div>}
                  {record.currentBannedBy && <div><strong>Banned by:</strong> {record.currentBannedBy}</div>}
                  {record.currentBanExpiresAt ? (
                    <div><strong>Expires:</strong> {formatDate(record.currentBanExpiresAt)}</div>
                  ) : (
                    <div><strong>Type:</strong> Permanent ban</div>
                  )}
                  {record.totalBansCount > 1 && <div><strong>Total bans:</strong> {record.totalBansCount}</div>}
                </div>
              }
              placement="topLeft"
              overlayStyle={{ maxWidth: 300 }}
            >
              <Tag color="volcano" icon={<InfoCircleOutlined />} style={{ cursor: 'pointer' }}>
                Banned
              </Tag>
            </Tooltip>
          ) : (
            <Tag color="green">Active</Tag>
          )}
        </div>
      ),
    },
    {
      title: 'Ban Details',
      key: 'banDetails',
      width: 200,
      render: (_, record) => {
        if (!record.isBanned) {
          return <Text type="secondary">Not banned</Text>;
        }
        
        return (
          <Space direction="vertical" size="small" style={{ fontSize: '12px' }}>
            {record.currentBanReason && (
              <div>
                <Text strong>Reason:</Text>
                <br />
                <Text style={{ fontSize: '11px' }}>{record.currentBanReason}</Text>
              </div>
            )}
            {record.currentBannedAt && (
              <div>
                <Text strong>Banned:</Text> <Text type="secondary">{formatDate(record.currentBannedAt)}</Text>
              </div>
            )}
            {record.totalBansCount > 1 && (
              <div>
                <Text strong>History:</Text> <Text type="secondary">{record.totalBansCount} total bans</Text>
              </div>
            )}
            <div>
              <Text strong>Status:</Text> <Text type="secondary">{getBanStatusText(record)}</Text>
            </div>
          </Space>
        );
      },
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
            <Tooltip title={`Remove ban: ${record.currentBanReason || 'No reason'}`}>
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

export default UsersPage;