import React, { useState } from 'react';
import { Button, List, Modal, Form, Input, Select, message, Popconfirm, Typography, Tag } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, HeartOutlined } from '@ant-design/icons';
import { useHobbies } from '../../../hooks/useProfile';
import type { Hobby } from '../../../types/profile';

const { Title, Text } = Typography;
const { TextArea } = Input;
const { Option } = Select;

const HobbiesTab: React.FC = () => {
  const { hobbies, loading, addHobby, updateHobby, deleteHobby } = useHobbies();
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [editingHobby, setEditingHobby] = useState<Hobby | null>(null);
  const [form] = Form.useForm();
  const [submitting, setSubmitting] = useState(false);

  const levelColors = {
    'Beginner': 'green',
    'Intermediate': 'blue',
    'Advanced': 'orange',
    'Expert': 'red'
  };

  const showModal = (hobby?: Hobby) => {
    setEditingHobby(hobby || null);
    setIsModalVisible(true);
    
    if (hobby) {
      form.setFieldsValue(hobby);
    } else {
      form.resetFields();
    }
  };

  const handleCancel = () => {
    setIsModalVisible(false);
    setEditingHobby(null);
    form.resetFields();
  };

  const handleSubmit = async (values: {
    name: string;
    description?: string;
    level?: 'Beginner' | 'Intermediate' | 'Advanced' | 'Expert';
  }) => {
    try {
      setSubmitting(true);

      if (editingHobby) {
        await updateHobby(editingHobby.id, values);
        message.success('Hobby updated successfully!');
      } else {
        await addHobby(values);
        message.success('Hobby added successfully!');
      }

      handleCancel();
    } catch {
      message.error(`Failed to ${editingHobby ? 'update' : 'add'} hobby`);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteHobby(id);
      message.success('Hobby deleted successfully!');
    } catch {
      message.error('Failed to delete hobby');
    }
  };

  return (
    <div className="hobbies-tab">
      <div className="hobbies-tab__header">
        <Title level={3}>Hobbies & Interests</Title>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => showModal()}
        >
          Add Hobby
        </Button>
      </div>

      <List
        loading={loading}
        dataSource={hobbies}
        renderItem={(hobby) => (
          <List.Item
            actions={[
              <Button
                key="edit"
                type="text"
                icon={<EditOutlined />}
                onClick={() => showModal(hobby)}
              />,
              <Popconfirm
                key="delete"
                title="Are you sure you want to delete this hobby?"
                onConfirm={() => handleDelete(hobby.id)}
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
              avatar={<HeartOutlined style={{ fontSize: '24px', color: 'var(--color-primary)' }} />}
              title={
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <Text strong>{hobby.name}</Text>
                  {hobby.level && (
                    <Tag color={levelColors[hobby.level]}>
                      {hobby.level}
                    </Tag>
                  )}
                </div>
              }
              description={hobby.description}
            />
          </List.Item>
        )}
        locale={{ emptyText: 'No hobbies added yet' }}
      />

      <Modal
        title={editingHobby ? 'Edit Hobby' : 'Add Hobby'}
        open={isModalVisible}
        onCancel={handleCancel}
        footer={null}
        width={500}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <Form.Item
            label="Hobby Name"
            name="name"
            rules={[{ required: true, message: 'Please enter the hobby name' }]}
          >
            <Input placeholder="e.g., Photography, Cooking, Reading" />
          </Form.Item>

          <Form.Item
            label="Skill Level"
            name="level"
          >
            <Select placeholder="Select your skill level">
              <Option value="Beginner">Beginner</Option>
              <Option value="Intermediate">Intermediate</Option>
              <Option value="Advanced">Advanced</Option>
              <Option value="Expert">Expert</Option>
            </Select>
          </Form.Item>

          <Form.Item
            label="Description"
            name="description"
          >
            <TextArea
              rows={3}
              placeholder="Tell us more about this hobby..."
              maxLength={200}
              showCount
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Button onClick={handleCancel} style={{ marginRight: 8 }}>
              Cancel
            </Button>
            <Button type="primary" htmlType="submit" loading={submitting}>
              {editingHobby ? 'Update' : 'Add'} Hobby
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default HobbiesTab;
