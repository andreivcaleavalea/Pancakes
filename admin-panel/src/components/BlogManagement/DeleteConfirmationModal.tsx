import React from 'react';
import { Modal, Form, Alert, Input } from 'antd';
import { ExclamationCircleOutlined } from '@ant-design/icons';
import { BlogPost } from '../../types';
import { format } from 'date-fns';
import './DeleteConfirmationModal.css';

const { TextArea } = Input;

interface DeleteConfirmationModalProps {
  visible: boolean;
  blog: BlogPost | null;
  loading: boolean;
  onSubmit: (values: { reason: string }) => void;
  onCancel: () => void;
}

const DeleteConfirmationModal: React.FC<DeleteConfirmationModalProps> = ({
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

  const formatDate = (dateString: string) => {
    return format(new Date(dateString), 'MMM dd, yyyy HH:mm');
  };

  return (
    <Modal
      title="Delete Blog Post"
      open={visible}
      onOk={form.submit}
      onCancel={handleCancel}
      confirmLoading={loading}
      okType="danger"
      okText="Delete"
    >
      {blog && (
        <div className="delete-modal-content">
          <Alert
            message="Warning"
            description={
              <div>
                <p>You are about to permanently delete this blog post:</p>
                <p><strong>Title:</strong> {blog.title}</p>
                <p><strong>Author:</strong> {blog.authorName}</p>
                <p><strong>Created:</strong> {formatDate(blog.createdAt)}</p>
                <p className="warning-text">
                  <ExclamationCircleOutlined /> This action cannot be undone!
                </p>
              </div>
            }
            type="warning"
            showIcon
          />
        </div>
      )}
      
      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
      >
        <Form.Item
          name="reason"
          label="Reason for Deletion"
          rules={[
            { required: true, message: 'Please provide a reason for deletion' },
            { min: 10, message: 'Reason must be at least 10 characters' }
          ]}
        >
          <TextArea 
            rows={3} 
            placeholder="Explain why you're deleting this blog post..."
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default DeleteConfirmationModal;
