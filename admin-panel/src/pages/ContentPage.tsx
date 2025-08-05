import React, { useState } from 'react';
import { Typography, Card, Input, Button, Select, Alert } from 'antd';
import { SearchOutlined, FileTextOutlined, ReloadOutlined } from '@ant-design/icons';
import { useBlogManagement } from '../hooks/useBlogManagement';
import { BlogPost, BlogPostStatus } from '../types';
import { 
  BlogPostTable, 
  StatusUpdateModal, 
  DeleteConfirmationModal 
} from '../components/BlogManagement';
import './ContentPage.css';

const { Title, Text } = Typography;
const { Option } = Select;

const ContentPage: React.FC = () => {
  const { 
    blogs, 
    pagination,
    loading, 
    actionLoading, 
    error, 
    search, 
    filterByStatus,
    changePage,
    deleteBlogPost,
    updateBlogStatus,
    refresh 
  } = useBlogManagement();

  const [statusModalVisible, setStatusModalVisible] = useState(false);
  const [deleteModalVisible, setDeleteModalVisible] = useState(false);
  const [selectedBlog, setSelectedBlog] = useState<BlogPost | null>(null);

  const handleSearch = (value: string) => {
    search(value.trim());
  };

  const handleStatusFilter = (status?: number) => {
    filterByStatus(status);
  };

  const handleUpdateStatus = (blog: BlogPost) => {
    setSelectedBlog(blog);
    setStatusModalVisible(true);
  };

  const handleDeleteBlog = (blog: BlogPost) => {
    setSelectedBlog(blog);
    setDeleteModalVisible(true);
  };

  const onStatusSubmit = async (values: { status: number; reason: string }) => {
    if (!selectedBlog) return;
    
    const { status, reason } = values;
    await updateBlogStatus(selectedBlog.id, status, reason);
    setStatusModalVisible(false);
    setSelectedBlog(null);
  };

  const onDeleteSubmit = async (values: { reason: string }) => {
    if (!selectedBlog) return;
    
    const { reason } = values;
    await deleteBlogPost(selectedBlog.id, reason);
    setDeleteModalVisible(false);
    setSelectedBlog(null);
  };

  const handleCloseStatusModal = () => {
    setStatusModalVisible(false);
    setSelectedBlog(null);
  };

  const handleCloseDeleteModal = () => {
    setDeleteModalVisible(false);
    setSelectedBlog(null);
  };

  return (
    <div>
      <div className="content-page-header">
        <Title level={2}>
          <FileTextOutlined /> Content Management
        </Title>
        <Text type="secondary">
          Manage and moderate blog posts across the platform
        </Text>
      </div>

      {error && (
        <Alert
          message="Error"
          description={error}
          type="error"
          showIcon
          className="content-page-error"
        />
      )}

      <Card>
        <div className="content-page-filters">
          <Input.Search
            placeholder="Search blog posts..."
            allowClear
            className="search-input"
            onSearch={handleSearch}
            enterButton={<SearchOutlined />}
            size="large"
          />
          
          <Select
            placeholder="Filter by status"
            allowClear
            className="status-filter"
            onChange={handleStatusFilter}
            size="large"
          >
            <Option value={BlogPostStatus.Draft}>Draft</Option>
            <Option value={BlogPostStatus.Published}>Published</Option>
            <Option value={BlogPostStatus.Deleted}>Deleted</Option>
          </Select>

          <Button 
            icon={<ReloadOutlined />} 
            onClick={refresh}
            loading={loading}
            size="large"
            className="refresh-button"
          >
            Refresh
          </Button>
        </div>

        <BlogPostTable
          blogs={blogs}
          loading={loading}
          actionLoading={actionLoading}
          pagination={pagination}
          onChangePage={changePage}
          onUpdateStatus={handleUpdateStatus}
          onDelete={handleDeleteBlog}
        />
      </Card>

      <StatusUpdateModal
        visible={statusModalVisible}
        blog={selectedBlog}
        loading={actionLoading}
        onSubmit={onStatusSubmit}
        onCancel={handleCloseStatusModal}
      />

      <DeleteConfirmationModal
        visible={deleteModalVisible}
        blog={selectedBlog}
        loading={actionLoading}
        onSubmit={onDeleteSubmit}
        onCancel={handleCloseDeleteModal}
      />
    </div>
  );
};

export default ContentPage;
