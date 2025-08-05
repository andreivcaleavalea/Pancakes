import React from 'react';
import { Modal, Form, Select, Tag, Input } from 'antd';
import { BlogPost, BlogPostStatus, BlogPostStatusLabels, BlogPostStatusColors } from '../../types';
import './StatusUpdateModal.css';

const { TextArea } = Input;
const { Option } = Select;

interface StatusUpdateModalProps {
  visible: boolean;
  blog: BlogPost | null;
  loading: boolean;
  onSubmit: (values: { status: number; reason: string }) => void;
  onCancel: () => void;
}

const StatusUpdateModal: React.FC<StatusUpdateModalProps> = ({
  visible,
  blog,
  loading,
  onSubmit,
  onCancel,
}) => {
  const [form] = Form.useForm();

  const handleSubmit = async (values: any) => {
    await onSubmit(values);
    form.resetFields();
  };

  const handleCancel = () => {
    form.resetFields();
    onCancel();
  };

  // Set initial values when blog changes
  React.useEffect(() => {
    if (blog) {
      form.setFieldsValue({ status: blog.status });
    }
  }, [blog, form]);

  return (
    <Modal
      title="Update Blog Post Status"
      open={visible}
      onOk={form.submit}
      onCancel={handleCancel}
      confirmLoading={loading}
    >
      {blog && (
        <div className="status-modal-content">
          <strong>Blog Post: </strong>
          {blog.title}
          <br />
          <strong>Current Status: </strong>
          <Tag color={BlogPostStatusColors[blog.status as BlogPostStatus]}>
            {BlogPostStatusLabels[blog.status as BlogPostStatus]}
          </Tag>
        </div>
      )}
      
      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
      >
        <Form.Item
          name="status"
          label="New Status"
          rules={[{ required: true, message: 'Please select a status' }]}
        >
          <Select>
            <Option value={BlogPostStatus.Draft}>
              <Tag color={BlogPostStatusColors[BlogPostStatus.Draft]}>Draft</Tag>
            </Option>
            <Option value={BlogPostStatus.Published}>
              <Tag color={BlogPostStatusColors[BlogPostStatus.Published]}>Published</Tag>
            </Option>
          </Select>
        </Form.Item>
        
        <Form.Item
          name="reason"
          label="Reason for Status Change"
          rules={[
            { required: true, message: 'Please provide a reason' },
            { min: 10, message: 'Reason must be at least 10 characters' }
          ]}
        >
          <TextArea 
            rows={3} 
            placeholder="Explain why you're changing the status..."
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default StatusUpdateModal;
