import React from 'react';
import { Card, Avatar, Typography, Row, Col, Space, Tag, Timeline } from 'antd';
import { usePersonalPagePreview } from '../../../hooks/usePersonalPage';
import { getProfilePictureUrl } from '../../../utils/imageUtils';
import type { PersonalPageSettings } from '../../../services/personalPageService';
import './PersonalPageView.scss';

const { Title, Text, Paragraph } = Typography;

interface PersonalPageViewProps {
  settings: PersonalPageSettings | null;
}

const PersonalPageView: React.FC<PersonalPageViewProps> = ({ settings }) => {
  const { pageData, loading, fetchPreview } = usePersonalPagePreview();

  React.useEffect(() => {
    if (settings?.pageSlug) {
      fetchPreview();
    }
  }, [settings?.pageSlug, fetchPreview]);

  if (!settings || !pageData) {
    return (
      <div className="personal-page-view__empty">
        <Card>
          <div style={{ textAlign: 'center', padding: '40px' }}>
            <Title level={3}>Personal Page Preview</Title>
            <Text type="secondary">Configure your settings to see how your page will look</Text>
          </div>
        </Card>
      </div>
    );
  }

  const { user, educations, jobs, hobbies, projects } = pageData;
  const { sectionOrder, sectionVisibility, sectionTemplates } = settings;

  const renderPersonalSection = () => {
    if (!sectionVisibility.personal) return null;

    return (
      <Card className="personal-page-view__section" key="personal">
        <div className="personal-page-view__hero">
          <Row gutter={[24, 24]} align="middle">
            <Col xs={24} sm={8} md={6}>
              <div className="personal-page-view__avatar-container">
                <Avatar
                  size={120}
                  src={getProfilePictureUrl(user.avatar)}
                  className="personal-page-view__avatar"
                />
              </div>
            </Col>
            <Col xs={24} sm={16} md={18}>
              <div className="personal-page-view__intro">
                <Title level={1} className="personal-page-view__name">
                  {user.name}
                </Title>
                <Text className="personal-page-view__email" type="secondary">
                  {user.email}
                </Text>
                {user.bio && (
                  <Paragraph className="personal-page-view__bio">
                    {user.bio}
                  </Paragraph>
                )}
                {user.phoneNumber && (
                  <Text type="secondary">📞 {user.phoneNumber}</Text>
                )}
              </div>
            </Col>
          </Row>
        </div>
      </Card>
    );
  };

  const renderEducationSection = () => {
    if (!sectionVisibility.education || !educations.length) return null;

    const template = sectionTemplates.education || 'timeline';

    return (
      <Card className="personal-page-view__section" key="education">
        <Title level={2}>🎓 Education</Title>
        {template === 'timeline' ? (
          <Timeline
            items={educations.map(edu => ({
              children: (
                <div>
                  <Title level={4}>{edu.institution}</Title>
                  <Text strong>{edu.degree} in {edu.specialization}</Text>
                  <br />
                  <Text type="secondary">
                    {edu.startDate} - {edu.endDate || 'Present'}
                  </Text>
                  {edu.description && <Paragraph>{edu.description}</Paragraph>}
                </div>
              )
            }))}
          />
        ) : (
          <Row gutter={[16, 16]}>
            {educations.map(edu => (
              <Col xs={24} md={12} key={edu.id}>
                <Card size="small">
                  <Title level={4}>{edu.institution}</Title>
                  <Text strong>{edu.degree} in {edu.specialization}</Text>
                  <br />
                  <Text type="secondary">
                    {edu.startDate} - {edu.endDate || 'Present'}
                  </Text>
                  {edu.description && <Paragraph>{edu.description}</Paragraph>}
                </Card>
              </Col>
            ))}
          </Row>
        )}
      </Card>
    );
  };

  const renderJobsSection = () => {
    if (!sectionVisibility.jobs || !jobs.length) return null;

    return (
      <Card className="personal-page-view__section" key="jobs">
        <Title level={2}>💼 Work Experience</Title>
        <Timeline
          items={jobs.map(job => ({
            children: (
              <div>
                <Title level={4}>{job.position}</Title>
                <Text strong>{job.company}</Text>
                {job.location && <Text type="secondary"> • {job.location}</Text>}
                <br />
                <Text type="secondary">
                  {job.startDate} - {job.endDate || 'Present'}
                </Text>
                {job.description && <Paragraph>{job.description}</Paragraph>}
              </div>
            )
          }))}
        />
      </Card>
    );
  };

  const renderProjectsSection = () => {
    if (!sectionVisibility.projects || !projects.length) return null;

    return (
      <Card className="personal-page-view__section" key="projects">
        <Title level={2}>🚀 Projects</Title>
        <Row gutter={[16, 16]}>
          {projects.map(project => (
            <Col xs={24} md={12} lg={8} key={project.id}>
              <Card 
                size="small" 
                className="personal-page-view__project-card"
                actions={[
                  project.projectUrl && <a href={project.projectUrl} target="_blank" rel="noopener noreferrer">View</a>,
                  project.githubUrl && <a href={project.githubUrl} target="_blank" rel="noopener noreferrer">GitHub</a>
                ].filter(Boolean)}
              >
                <Title level={4}>{project.name}</Title>
                <Paragraph>{project.description}</Paragraph>
                {project.technologies && (
                  <div>
                    {project.technologies.split(',').map((tech, index) => (
                      <Tag key={index} color="blue">{tech.trim()}</Tag>
                    ))}
                  </div>
                )}
              </Card>
            </Col>
          ))}
        </Row>
      </Card>
    );
  };

  const renderHobbiesSection = () => {
    if (!sectionVisibility.hobbies || !hobbies.length) return null;

    return (
      <Card className="personal-page-view__section" key="hobbies">
        <Title level={2}>🎯 Hobbies & Interests</Title>
        <Row gutter={[16, 16]}>
          {hobbies.map(hobby => (
            <Col xs={24} sm={12} md={8} key={hobby.id}>
              <Card size="small">
                <Title level={4}>{hobby.name}</Title>
                <Tag color="orange">{hobby.level}</Tag>
                {hobby.description && <Paragraph>{hobby.description}</Paragraph>}
              </Card>
            </Col>
          ))}
        </Row>
      </Card>
    );
  };

  const sectionRenderers: Record<string, () => React.ReactNode> = {
    personal: renderPersonalSection,
    education: renderEducationSection,
    jobs: renderJobsSection,
    projects: renderProjectsSection,
    hobbies: renderHobbiesSection,
  };

  return (
    <div className="personal-page-view">
      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        {sectionOrder.map(sectionKey => sectionRenderers[sectionKey]?.())}
      </Space>
    </div>
  );
};

export default PersonalPageView; 