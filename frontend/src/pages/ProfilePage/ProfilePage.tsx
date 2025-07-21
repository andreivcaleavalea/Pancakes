import React, { useState } from 'react';
import { Card, Tabs, Spin, Alert, Avatar, Typography, Row, Col, Button } from 'antd';
import { UserOutlined, EditOutlined } from '@ant-design/icons';
import { useProfile } from '../../hooks/useProfile';
import { getProfilePictureUrl } from '../../utils/imageUtils';
import UserInfoTab from './components/UserInfoTab';
import EducationTab from './components/EducationTab';
import JobsTab from './components/JobsTab';
import HobbiesTab from './components/HobbiesTab';
import ProjectsTab from './components/ProjectsTab';
import './ProfilePage.scss';

const { Title, Text } = Typography;

const ProfilePage: React.FC = () => {
  const { profileData, loading, error } = useProfile();
  const [activeTab, setActiveTab] = useState('userInfo');



  if (loading) {
    return (
      <div className="profile-page__loading">
        <Spin size="large" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="profile-page__error">
        <Alert
          message="Error Loading Profile"
          description={error}
          type="error"
          showIcon
        />
      </div>
    );
  }

  if (!profileData) {
    return (
      <div className="profile-page__error">
        <Alert
          message="No Profile Data"
          description="Unable to load profile information."
          type="warning"
          showIcon
        />
      </div>
    );
  }

  const { user } = profileData;

  const tabItems = [
    {
      key: 'userInfo',
      label: 'Personal Info',
      children: <UserInfoTab />,
    },
    {
      key: 'education',
      label: 'Education',
      children: <EducationTab />,
    },
    {
      key: 'jobs',
      label: 'Work Experience',
      children: <JobsTab />,
    },
    {
      key: 'hobbies',
      label: 'Hobbies',
      children: <HobbiesTab />,
    },
    {
      key: 'projects',
      label: 'Projects',
      children: <ProjectsTab />,
    },
  ];

  return (
    <div className="profile-page">
      <div className="profile-page__header">
        <Card className="profile-page__header-card">
          <Row gutter={24} align="middle">
            <Col>
              <Avatar
                size={120}
                src={getProfilePictureUrl(user.avatar)}
                icon={<UserOutlined />}
                className="profile-page__avatar"
              />
            </Col>
            <Col flex="auto">
              <div className="profile-page__user-info">
                <Title level={2} className="profile-page__name">
                  {user.name}
                </Title>
                <Text type="secondary" className="profile-page__email">
                  {user.email}
                </Text>
                {user.bio && (
                  <Text className="profile-page__bio">
                    {user.bio}
                  </Text>
                )}
              </div>
            </Col>
            <Col>
              <Button 
                type="primary" 
                icon={<EditOutlined />}
                onClick={() => setActiveTab('userInfo')}
              >
                Edit Profile
              </Button>
            </Col>
          </Row>
        </Card>
      </div>

      <div className="profile-page__content">
        <Card className="profile-page__tabs-card">
          <Tabs
            activeKey={activeTab}
            onChange={setActiveTab}
            type="card"
            className="profile-page__tabs"
            items={tabItems}
          />
        </Card>
      </div>
    </div>
  );
};

export default ProfilePage;
