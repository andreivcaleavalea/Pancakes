import React, { useState } from 'react';
import { Form, Input, Button, DatePicker, Avatar, Upload, Row, Col, message } from 'antd';
import { UserOutlined, UploadOutlined, SaveOutlined } from '@ant-design/icons';
import { useProfile } from '../../../hooks/useProfile';
import type { UserProfile } from '../../../types/profile';
import dayjs from 'dayjs';

const { TextArea } = Input;

const UserInfoTab: React.FC = () => {
  const { profileData, updateUserProfile } = useProfile();
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);

  if (!profileData) {
    return <div>No profile data available</div>;
  }

  const { user } = profileData;

  const handleSubmit = async (values: Record<string, any>) => {
    try {
      setLoading(true);
      // Only send fields that can be updated (exclude email and handle avatar properly)
      const updatedData: Partial<UserProfile> = {
        name: values.name,
        bio: values.bio || '',
        phoneNumber: values.phoneNumber || '',
        dateOfBirth: values.dateOfBirth ? values.dateOfBirth.format('YYYY-MM-DD') : undefined
      };
      
      console.log('Form values received:', values);
      console.log('Submitting profile update:', updatedData);
      await updateUserProfile(updatedData);
      message.success('Profile updated successfully!');
    } catch (error) {
      console.error('Profile update failed:', error);
      const errorMessage = error instanceof Error ? error.message : 'Failed to update profile';
      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const initialValues = {
    ...user,
    dateOfBirth: user.dateOfBirth ? dayjs(user.dateOfBirth) : undefined
  };

  return (
    <div className="user-info-tab">
      <Row gutter={24}>
        <Col xs={24} md={8}>
          <div className="user-info-tab__avatar-section">
            <Avatar
              size={120}
              src={user.avatar}
              icon={<UserOutlined />}
              className="user-info-tab__avatar"
            />
            <Upload
              showUploadList={false}
              beforeUpload={() => {
                message.info('Avatar upload will be implemented soon');
                return false;
              }}
            >
              <Button icon={<UploadOutlined />} className="user-info-tab__upload-btn">
                Change Avatar
              </Button>
            </Upload>
          </div>
        </Col>
        <Col xs={24} md={16}>
          <Form
            form={form}
            layout="vertical"
            initialValues={initialValues}
            onFinish={handleSubmit}
            className="user-info-tab__form"
          >
            <Row gutter={16}>
              <Col xs={24} md={12}>
                <Form.Item
                  label="Full Name"
                  name="name"
                  rules={[
                    { required: true, message: 'Name is required' },
                    { min: 2, message: 'Name must be at least 2 characters' },
                    { max: 255, message: 'Name cannot exceed 255 characters' },
                    { pattern: /^[a-zA-Z\s\-\.]+$/, message: 'Name can only contain letters, spaces, hyphens, and periods' }
                  ]}
                >
                  <Input placeholder="Enter your full name" maxLength={255} />
                </Form.Item>
              </Col>
              <Col xs={24} md={12}>
                <Form.Item
                  label="Email"
                  name="email"
                  rules={[
                    { required: true, message: 'Please enter your email' },
                    { type: 'email', message: 'Please enter a valid email' }
                  ]}
                >
                  <Input placeholder="Enter your email" disabled />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col xs={24} md={12}>
                <Form.Item
                  label="Phone Number"
                  name="phoneNumber"
                  rules={[
                    { max: 20, message: 'Phone number cannot exceed 20 characters' },
                    { pattern: /^[\+]?[0-9][\d]{0,15}$/, message: 'Please enter a valid phone number' }
                  ]}
                >
                  <Input placeholder="Enter your phone number (e.g., +1234567890)" maxLength={20} />
                </Form.Item>
              </Col>
              <Col xs={24} md={12}>
                <Form.Item
                  label="Date of Birth"
                  name="dateOfBirth"
                >
                  <DatePicker 
                    placeholder="Select date of birth"
                    style={{ width: '100%' }}
                    format="DD/MM/YYYY"
                  />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item
              label="Bio"
              name="bio"
              rules={[
                { max: 1000, message: 'Bio cannot exceed 1000 characters' }
              ]}
            >
              <TextArea
                rows={4}
                placeholder="Tell us about yourself..."
                maxLength={1000}
                showCount
              />
            </Form.Item>

            <Form.Item>
              <Button
                type="primary"
                htmlType="submit"
                loading={loading}
                icon={<SaveOutlined />}
                size="large"
              >
                Save Changes
              </Button>
            </Form.Item>
          </Form>
        </Col>
      </Row>
    </div>
  );
};

export default UserInfoTab;
