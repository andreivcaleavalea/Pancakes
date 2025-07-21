import React, { useState } from 'react';
import { Button, List, Modal, Form, Input, DatePicker, Row, Col, message, Popconfirm, Typography } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, BankOutlined } from '@ant-design/icons';
import { useJobs } from '../../../hooks/useProfile';
import type { Job } from '../../../types/profile';
import dayjs, { type Dayjs } from 'dayjs';

const { Title, Text } = Typography;
const { TextArea } = Input;

const JobsTab: React.FC = () => {
  const { jobs, loading, addJob, updateJob, deleteJob } = useJobs();
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [editingJob, setEditingJob] = useState<Job | null>(null);
  const [form] = Form.useForm();
  const [submitting, setSubmitting] = useState(false);

  const showModal = (job?: Job) => {
    setEditingJob(job || null);
    setIsModalVisible(true);
    
    if (job) {
      form.setFieldsValue({
        ...job,
        startDate: dayjs(job.startDate),
        endDate: job.endDate ? dayjs(job.endDate) : undefined
      });
    } else {
      form.resetFields();
    }
  };

  const handleCancel = () => {
    setIsModalVisible(false);
    setEditingJob(null);
    form.resetFields();
  };

  const handleSubmit = async (values: {
    company: string;
    position: string;
    startDate: Dayjs;
    endDate?: Dayjs;
    description?: string;
    location?: string;
  }) => {
    try {
      setSubmitting(true);
      const jobData = {
        company: values.company,
        position: values.position,
        description: values.description,
        location: values.location,
        startDate: values.startDate.format('YYYY-MM'),
        endDate: values.endDate ? values.endDate.format('YYYY-MM') : undefined
      };

      if (editingJob) {
        await updateJob(editingJob.id, jobData);
        message.success('Job updated successfully!');
      } else {
        await addJob(jobData);
        message.success('Job added successfully!');
      }

      handleCancel();
    } catch {
      message.error(`Failed to ${editingJob ? 'update' : 'add'} job`);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteJob(id);
      message.success('Job deleted successfully!');
    } catch {
      message.error('Failed to delete job');
    }
  };

  const formatDate = (date: string) => {
    return dayjs(date).format('MMM YYYY');
  };

  const getPeriod = (startDate: string, endDate?: string) => {
    const start = formatDate(startDate);
    const end = endDate ? formatDate(endDate) : 'Present';
    return `${start} - ${end}`;
  };

  return (
    <div className="jobs-tab">
      <div className="jobs-tab__header">
        <Title level={3}>Work Experience</Title>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => showModal()}
        >
          Add Job
        </Button>
      </div>

      <List
        loading={loading}
        dataSource={jobs}
        renderItem={(job) => (
          <List.Item
            actions={[
              <Button
                key="edit"
                type="text"
                icon={<EditOutlined />}
                onClick={() => showModal(job)}
              />,
              <Popconfirm
                key="delete"
                title="Are you sure you want to delete this job?"
                onConfirm={() => handleDelete(job.id)}
                okText="Yes"
                cancelText="No"
              >
                <Button
                  type="text"
                  danger
                  icon={<DeleteOutlined />}
                />
              </Popconfirm>
            ]}
          >
            <List.Item.Meta
              avatar={<BankOutlined style={{ fontSize: '24px', color: 'var(--color-primary)' }} />}
              title={
                <div>
                  <Text strong>{job.position}</Text>
                  <Text type="secondary" style={{ marginLeft: '8px' }}>
                    at {job.company}
                  </Text>
                </div>
              }
              description={
                <div>
                  <Text type="secondary">{getPeriod(job.startDate, job.endDate)}</Text>
                  {job.location && (
                    <>
                      <Text type="secondary"> • </Text>
                      <Text type="secondary">{job.location}</Text>
                    </>
                  )}
                  {job.description && (
                    <>
                      <br />
                      <Text>{job.description}</Text>
                    </>
                  )}
                </div>
              }
            />
          </List.Item>
        )}
        locale={{ emptyText: 'No work experience added yet' }}
      />

      <Modal
        title={editingJob ? 'Edit Job' : 'Add Job'}
        open={isModalVisible}
        onCancel={handleCancel}
        footer={null}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <Form.Item
            label="Company"
            name="company"
            rules={[
              { required: true, message: 'Company name is required' },
              { min: 2, message: 'Company name must be at least 2 characters' },
              { max: 255, message: 'Company name cannot exceed 255 characters' }
            ]}
          >
            <Input placeholder="e.g., TechCorp SRL" maxLength={255} />
          </Form.Item>

          <Form.Item
            label="Position"
            name="position"
            rules={[
              { required: true, message: 'Position is required' },
              { min: 2, message: 'Position must be at least 2 characters' },
              { max: 255, message: 'Position cannot exceed 255 characters' }
            ]}
          >
            <Input placeholder="e.g., Full-Stack Developer" maxLength={255} />
          </Form.Item>

          <Form.Item
            label="Location"
            name="location"
            rules={[
              { max: 255, message: 'Location cannot exceed 255 characters' }
            ]}
          >
            <Input placeholder="e.g., București, România" maxLength={255} />
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                label="Start Date"
                name="startDate"
                rules={[{ required: true, message: 'Please select start date' }]}
              >
                <DatePicker
                  picker="month"
                  placeholder="Select start date"
                  style={{ width: '100%' }}
                  format="MM/YYYY"
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                label="End Date"
                name="endDate"
              >
                <DatePicker
                  picker="month"
                  placeholder="Select end date (leave empty if current)"
                  style={{ width: '100%' }}
                  format="MM/YYYY"
                />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            label="Job Description"
            name="description"
            rules={[
              { max: 1000, message: 'Description cannot exceed 1000 characters' }
            ]}
          >
            <TextArea
              rows={4}
              placeholder="Describe your responsibilities and achievements..."
              maxLength={1000}
              showCount
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Button onClick={handleCancel} style={{ marginRight: 8 }}>
              Cancel
            </Button>
            <Button type="primary" htmlType="submit" loading={submitting}>
              {editingJob ? 'Update' : 'Add'} Job
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default JobsTab;
