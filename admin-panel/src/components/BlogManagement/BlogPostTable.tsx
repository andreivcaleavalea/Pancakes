import React from 'react';
import { Table, Space, Tooltip, Button, Tag, Avatar, Image, Typography } from 'antd';
import { EyeOutlined, EditOutlined, DeleteOutlined, UserOutlined, CalendarOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { BlogPost, BlogPostStatus, BlogPostStatusLabels, BlogPostStatusColors } from '../../types';
import { format } from 'date-fns';
import './BlogPostTable.css';

const { Title, Text, Paragraph } = Typography;

interface BlogPostTableProps {
  blogs: BlogPost[];
  loading: boolean;
  actionLoading: boolean;
  pagination: any;
  onChangePage: (page: number, pageSize?: number) => void;
  onUpdateStatus: (blog: BlogPost) => void;
  onDelete: (blog: BlogPost) => void;
}

const BlogPostTable: React.FC<BlogPostTableProps> = ({
  blogs,
  loading,
  actionLoading,
  pagination,
  onChangePage,
  onUpdateStatus,
  onDelete,
}) => {
  const formatDate = (dateString: string) => {
    return format(new Date(dateString), 'MMM dd, yyyy HH:mm');
  };

  const truncateText = (text: string, maxLength: number = 100) => {
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
  };

  const handleViewPost = (blog: BlogPost) => {
    const frontendUrl = `http://localhost:3000/blog/${blog.id}`;
    window.open(frontendUrl, '_blank');
  };

  const columns: ColumnsType<BlogPost> = [
    {
      title: 'Post',
      key: 'post',
      width: 300,
      render: (_, record) => (
        <div className="blog-post-cell">
          {record.featuredImage && (
            <Image
              width={60}
              height={60}
              src={record.featuredImage}
              className="featured-image"
              fallback="data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCA2MCA2MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjYwIiBoZWlnaHQ9IjYwIiBmaWxsPSIjZjBmMGYwIi8+CjxwYXRoIGQ9Ik0yMCAyMEg0MFY0MEgyMFYyMFoiIGZpbGw9IiNkOWQ5ZDkiLz4KPC9zdmc+"
            />
          )}
          <div className="post-content">
            <Title level={5} className="post-title">
              {truncateText(record.title, 50)}
            </Title>
            <Paragraph className="post-excerpt">
              {truncateText(record.content.replace(/<[^>]*>/g, ''), 80)}
            </Paragraph>
            <div className="post-date">
              <CalendarOutlined /> {formatDate(record.createdAt)}
            </div>
          </div>
        </div>
      ),
    },
    {
      title: 'Author',
      key: 'author',
      width: 150,
      render: (_, record) => (
        <div className="author-cell">
          <Avatar 
            size={32}
            src={record.authorImage}
            icon={<UserOutlined />}
          />
          <div className="author-info">
            <div className="author-name">{record.authorName}</div>
            <div className="author-id">ID: {record.authorId.substring(0, 8)}...</div>
          </div>
        </div>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      filters: [
        { text: 'Draft', value: BlogPostStatus.Draft },
        { text: 'Published', value: BlogPostStatus.Published },
        { text: 'Deleted', value: BlogPostStatus.Deleted },
      ],
      onFilter: (value, record) => record.status === value,
      render: (status: number) => (
        <Tag color={BlogPostStatusColors[status as BlogPostStatus]}>
          {BlogPostStatusLabels[status as BlogPostStatus]}
        </Tag>
      ),
    },
    {
      title: 'Published',
      dataIndex: 'publishedAt',
      key: 'publishedAt',
      width: 120,
      render: (publishedAt?: string) => (
        publishedAt ? (
          <div className="date-cell">{formatDate(publishedAt)}</div>
        ) : (
          <Text type="secondary" className="date-cell">Not published</Text>
        )
      ),
    },
    {
      title: 'Last Updated',
      dataIndex: 'updatedAt',
      key: 'updatedAt',
      width: 120,
      render: (updatedAt: string) => (
        <div className="date-cell">{formatDate(updatedAt)}</div>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_, record) => (
        <Space size="small" className="actions-cell">
          <Tooltip title="View Details">
            <Button 
              type="text" 
              size="small" 
              icon={<EyeOutlined />}
              onClick={() => handleViewPost(record)}
              className="action-button"
            />
          </Tooltip>
          <Tooltip title="Update Status">
            <Button 
              type="text" 
              size="small" 
              icon={<EditOutlined />}
              onClick={() => onUpdateStatus(record)}
              loading={actionLoading}
              className="action-button"
            />
          </Tooltip>
          <Tooltip title="Delete Post">
            <Button 
              type="text" 
              size="small" 
              danger 
              icon={<DeleteOutlined />}
              onClick={() => onDelete(record)}
              loading={actionLoading}
              className="action-button"
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  return (
    <div className="responsive-table">
      <Table
        columns={columns}
        dataSource={blogs}
        rowKey="id"
        loading={loading}
        pagination={{
          ...pagination,
          onChange: onChangePage,
          showSizeChanger: true,
          showQuickJumper: true,
          showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} items`,
          responsive: true,
        }}
        scroll={{ x: 800 }}
        size="middle"
      />
    </div>
  );
};

export default BlogPostTable;
