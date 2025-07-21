import React, { useState } from 'react';
import { Button, List, Modal, Form, Input, DatePicker, Row, Col, message, Popconfirm, Typography } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, BookOutlined } from '@ant-design/icons';
import { useEducations } from '../../../hooks/useProfile';
import type { Education } from '../../../types/profile';
import dayjs, { type Dayjs } from 'dayjs';

const { Title, Text } = Typography;
const { TextArea } = Input;

const EducationTab: React.FC = () => {
  const { educations, loading, addEducation, updateEducation, deleteEducation } = useEducations();
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [editingEducation, setEditingEducation] = useState<Education | null>(null);
  const [form] = Form.useForm();
  const [submitting, setSubmitting] = useState(false);

  const showModal = (education?: Education) => {
    setEditingEducation(education || null);
    setIsModalVisible(true);
    
    if (education) {
      form.setFieldsValue({
        ...education,
        startDate: dayjs(education.startDate),
        endDate: education.endDate ? dayjs(education.endDate) : undefined
      });
    } else {
      form.resetFields();
    }
  };

  const handleCancel = () => {
    setIsModalVisible(false);
    setEditingEducation(null);
    form.resetFields();
  };

  const handleSubmit = async (values: {
    institution: string;
    specialization: string;
    degree: string;
    startDate: Dayjs;
    endDate?: Dayjs;
    description?: string;
  }) => {
    try {
      setSubmitting(true);
      const educationData = {
        institution: values.institution,
        specialization: values.specialization,
        degree: values.degree,
        description: values.description,
        startDate: values.startDate.format('YYYY-MM'),
        endDate: values.endDate ? values.endDate.format('YYYY-MM') : undefined
      };

      if (editingEducation) {
        await updateEducation(editingEducation.id, educationData);
        message.success('Education updated successfully!');
      } else {
        await addEducation(educationData);
        message.success('Education added successfully!');
      }

      handleCancel();
    } catch {
      message.error(`Failed to ${editingEducation ? 'update' : 'add'} education`);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteEducation(id);
      message.success('Education deleted successfully!');
    } catch {
      message.error('Failed to delete education');
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
    <div className="education-tab">
      <div className="education-tab__header">
        <Title level={3}>Education</Title>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => showModal()}
        >
          Add Education
        </Button>
      </div>

      <List
        loading={loading}
        dataSource={educations}
        renderItem={(education) => (
          <List.Item
            actions={[
              <Button
                key="edit"
                type="text"
                icon={<EditOutlined />}
                onClick={() => showModal(education)}
              />,
              <Popconfirm
                key="delete"
                title="Are you sure you want to delete this education?"
                onConfirm={() => handleDelete(education.id)}
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
              avatar={<BookOutlined style={{ fontSize: '24px', color: 'var(--color-primary)' }} />}
              title={
                <div>
                  <Text strong>{education.degree}</Text>
                  <Text type="secondary" style={{ marginLeft: '8px' }}>
                    at {education.institution}
                  </Text>
                </div>
              }
              description={
                <div>
                  <Text>{education.specialization}</Text>
                  <br />
                  <Text type="secondary">{getPeriod(education.startDate, education.endDate)}</Text>
                  {education.description && (
                    <>
                      <br />
                      <Text>{education.description}</Text>
                    </>
                  )}
                </div>
              }
            />
          </List.Item>
        )}
        locale={{ emptyText: 'No education added yet' }}
      />

      <Modal
        title={editingEducation ? 'Edit Education' : 'Add Education'}
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
            label="Institution"
            name="institution"
            rules={[
              { required: true, message: 'Institution name is required' },
              { min: 2, message: 'Institution name must be at least 2 characters' },
              { max: 255, message: 'Institution name cannot exceed 255 characters' }
            ]}
          >
            <Input placeholder="e.g., Universitatea Politehnica București" maxLength={255} />
          </Form.Item>

          <Form.Item
            label="Specialization"
            name="specialization"
            rules={[
              { required: true, message: 'Specialization is required' },
              { min: 2, message: 'Specialization must be at least 2 characters' },
              { max: 255, message: 'Specialization cannot exceed 255 characters' }
            ]}
          >
            <Input placeholder="e.g., Computer Science" maxLength={255} />
          </Form.Item>

          <Form.Item
            label="Degree"
            name="degree"
            rules={[
              { required: true, message: 'Degree is required' },
              { min: 2, message: 'Degree must be at least 2 characters' },
              { max: 100, message: 'Degree cannot exceed 100 characters' }
            ]}
          >
            <Input placeholder="e.g., Bachelor of Science" maxLength={100} />
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
            label="Description"
            name="description"
            rules={[
              { max: 1000, message: 'Description cannot exceed 1000 characters' }
            ]}
          >
            <TextArea
              rows={3}
              placeholder="Additional details about your education..."
              maxLength={1000}
              showCount
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Button onClick={handleCancel} style={{ marginRight: 8 }}>
              Cancel
            </Button>
            <Button type="primary" htmlType="submit" loading={submitting}>
              {editingEducation ? 'Update' : 'Add'} Education
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default EducationTab;
