import React, { useState, useEffect } from 'react';
import { Typography, Spin, Alert, Button } from 'antd';
import { HomeOutlined } from '@ant-design/icons';
import { PersonalSection, EducationSection, JobsSection, ProjectsSection, HobbiesSection } from './components/sections';
import { PersonalPageService, type PublicPersonalPage } from '../../services/personalPageService';
import type { SectionVisibility } from './types';
import './PersonalPage.scss';

const { Text, Title } = Typography;

interface PublicPersonalPageProps {
  pageSlug: string;
}

const PublicPersonalPageComponent: React.FC<PublicPersonalPageProps> = ({ pageSlug }) => {
  const [publicPage, setPublicPage] = useState<PublicPersonalPage | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!pageSlug) return;

    const fetchData = async () => {
      try {
        setLoading(true);
        const data = await PersonalPageService.getPublicPage(pageSlug);
        setPublicPage(data);
      } catch (err) {
        console.error('Error fetching public page:', err);
        setError(err instanceof Error ? err.message : 'Failed to load page');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [pageSlug]);

  if (loading) {
    return (
      <div style={{ padding: '24px', textAlign: 'center' }}>
        <Spin size="large" />
        <div style={{ marginTop: '16px' }}>
          <Text type="secondary">Loading personal page...</Text>
        </div>
      </div>
    );
  }

  if (error || !publicPage) {
    return (
      <div style={{ padding: '24px', textAlign: 'center' }}>
        <Alert
          message="Page Not Found"
          description={error || "This personal page doesn't exist or is not public."}
          type="error"
          showIcon
          style={{ marginBottom: '16px' }}
        />
        <Button 
          type="primary" 
          icon={<HomeOutlined />}
          onClick={() => window.location.href = '/'}
        >
          Go to Home
        </Button>
      </div>
    );
  }

  const { user, educations, jobs, projects, hobbies, settings } = publicPage;
  const { sectionOrder, sectionVisibility } = settings;

  // Get section data helper
  const getSectionData = (sectionKey: string) => {
    switch (sectionKey) {
      case 'personal':
        return user;
      case 'education':
        return educations;
      case 'jobs':
        return jobs;
      case 'projects':
        return projects;
      case 'hobbies':
        return hobbies;
      default:
        return [];
    }
  };

  // Render section helper
  const renderSection = (sectionKey: string) => {
    const data = getSectionData(sectionKey);
    const currentSectionSettings = {
      template: settings.sectionTemplates?.[sectionKey] || 'card',
      color: settings.sectionColors?.[sectionKey] || 'blue',
      advancedSettings: settings.sectionAdvancedSettings?.[sectionKey],
    };

    const sectionProps = {
      sectionKey,
      primaryColor: '#1890ff',
      currentSectionSettings,
      onSectionSettingsChange: () => {}, // No-op for public view
      templateOptions: [], // Empty for public view
      editMode: false, // Disable edit mode for public view
    };

    // Skip if section is not visible
    if (!sectionVisibility[sectionKey as keyof SectionVisibility]) {
      return null;
    }

    // Add specific data props for each section type
    switch (sectionKey) {
      case 'personal':
        return user ? (
          <PersonalSection
            key={sectionKey}
            {...sectionProps}
            user={user}
          />
        ) : null;
      case 'education':
        return (
          <EducationSection
            key={sectionKey}
            {...sectionProps}
            educations={educations}
          />
        );
      case 'jobs':
        return (
          <JobsSection
            key={sectionKey}
            {...sectionProps}
            jobs={jobs}
          />
        );
      case 'projects':
        return (
          <ProjectsSection
            key={sectionKey}
            {...sectionProps}
            projects={projects}
          />
        );
      case 'hobbies':
        return (
          <HobbiesSection
            key={sectionKey}
            {...sectionProps}
            hobbies={hobbies}
          />
        );
      default:
        return null;
    }
  };

  return (
    <div className="personal-page" style={{ padding: '24px', maxWidth: '1200px', margin: '0 auto' }}>
      {/* Render sections in order - clean public view */}
      <div style={{ width: '100%' }}>
        {sectionOrder.map((sectionKey: string) => renderSection(sectionKey))}
      </div>
    </div>
  );
};

export default PublicPersonalPageComponent; 