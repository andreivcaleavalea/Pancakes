import React from 'react';
import { Card, Typography, Spin, Alert, Button, Space } from 'antd';
import { SaveOutlined, UndoOutlined } from '@ant-design/icons';
import { PersonalSection, EducationSection, JobsSection, ProjectsSection, HobbiesSection } from './components/sections';
import SectionOrderControl from './components/SectionOrderControl';
import { usePersonalPageEditMode } from './hooks/usePersonalPageEditMode';
import type { SectionVisibility, User, Education, Job, Project, Hobby } from './types';
import './PersonalPage.scss';

const { Text } = Typography;

interface PersonalPageProps {
  // No external props needed - using internal edit mode
}

// Section components imported directly

const PersonalPage: React.FC<PersonalPageProps> = () => {
  const {
    user,
    educations,
    jobs,
    projects,
    hobbies,
    loading,
    error,
    sectionOrder,
    sectionVisibility,
    colorScheme,
    hasUnsavedChanges,
    saving,
    handleSave,
    handleRevert,
    handleSectionOrderChange,
    handleSectionVisibilityChange,
    getSectionProps,
  } = usePersonalPageEditMode();

  if (loading) {
    return (
      <div style={{ padding: '24px', textAlign: 'center' }}>
        <Spin size="large" />
        <div style={{ marginTop: '16px' }}>
          <Text type="secondary">Loading your personal page...</Text>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div style={{ padding: '24px' }}>
        <Alert
          message="Unable to load profile data"
          description={error || "Please check your connection and try again."}
          type="error"
          showIcon
        />
      </div>
    );
  }

  if (!loading && !user) {
    return (
      <div style={{ padding: '24px' }}>
        <Alert
          message="No profile data found"
          description="Please create your profile first."
          type="warning"
          showIcon
        />
      </div>
    );
  }

  const renderSection = (sectionKey: string) => {
    // Check if section should be visible
    if (!sectionVisibility[sectionKey as keyof SectionVisibility]) {
      return null;
    }

    // Component selection handled in switch statement below

    // Get section props from hook
    const sectionProps = getSectionProps(sectionKey);

    // Get section-specific data with proper typing
    let data: User | Education[] | Job[] | Project[] | Hobby[] | null = null;
    let shouldSkip = false;

    switch (sectionKey) {
      case 'personal':
        data = user;
        shouldSkip = !user;
        break;
      case 'education':
        data = educations || [];
        shouldSkip = data.length === 0;
        break;
      case 'jobs':
        data = jobs || [];
        shouldSkip = data.length === 0;
        break;
      case 'projects':
        data = projects || [];
        shouldSkip = data.length === 0;
        break;
      case 'hobbies':
        data = hobbies || [];
        shouldSkip = data.length === 0;
        break;
      default:
        shouldSkip = true;
    }

    // Skip rendering if no data
    if (shouldSkip) {
      return null;
    }

    // For personal section, pass user directly
    if (sectionKey === 'personal' && user) {
      return (
        <PersonalSection
          key={sectionKey}
          {...sectionProps}
          user={user}
        />
      );
    }

    // For other sections, pass data array with specific props
    const sectionSpecificProps = { ...sectionProps };
    
    // Add specific data props for each section type with proper typing
    switch (sectionKey) {
      case 'education':
        (sectionSpecificProps as any).educations = data as Education[];
        return (
          <EducationSection
            key={sectionKey}
            {...sectionSpecificProps}
            educations={data as Education[]}
          />
        );
      case 'jobs':
        (sectionSpecificProps as any).jobs = data as Job[];
        return (
          <JobsSection
            key={sectionKey}
            {...sectionSpecificProps}
            jobs={data as Job[]}
          />
        );
      case 'projects':
        (sectionSpecificProps as any).projects = data as Project[];
        return (
          <ProjectsSection
            key={sectionKey}
            {...sectionSpecificProps}
            projects={data as Project[]}
          />
        );
      case 'hobbies':
        (sectionSpecificProps as any).hobbies = data as Hobby[];
        return (
          <HobbiesSection
            key={sectionKey}
            {...sectionSpecificProps}
            hobbies={data as Hobby[]}
          />
        );
    }

    return null;
  };

  return (
    <div className="personal-page" style={{ padding: '24px' }}>
      <div style={{ width: '100%' }}>
        {/* DEBUG: Always render section order for testing */}
        <div style={{ 
          padding: '16px', 
          backgroundColor: '#f0f0f0', 
          marginBottom: '16px',
          border: '2px solid red' 
        }}>
          <h3>🔍 DEBUG: Section Order Should Be Here</h3>
          <p>sectionOrder: {JSON.stringify(sectionOrder)}</p>
          <p>handleSectionOrderChange: {typeof handleSectionOrderChange}</p>
        </div>
        
        {/* Section Order Control */}
        <SectionOrderControl
          sectionOrder={sectionOrder}
          onSectionOrderChange={handleSectionOrderChange}
          sectionVisibility={{
            personal: sectionVisibility.personal,
            education: sectionVisibility.education,
            jobs: sectionVisibility.jobs,
            projects: sectionVisibility.projects,
            hobbies: sectionVisibility.hobbies,
          }}
          onSectionVisibilityChange={handleSectionVisibilityChange}
        />
        
        {/* Render sections in order */}
        {sectionOrder.map((sectionKey: string) => renderSection(sectionKey))}
        
        {/* Save/Revert Action Buttons */}
        <Card style={{ 
          marginTop: '32px', 
          textAlign: 'center', 
          background: hasUnsavedChanges ? '#fff7e6' : '#fafafa', 
          borderColor: hasUnsavedChanges ? '#ffa940' : '#d9d9d9' 
        }}>
          {hasUnsavedChanges && (
            <Text type="warning" style={{ display: 'block', marginBottom: '16px', fontWeight: 500 }}>
              ⚠️ You have unsaved changes
            </Text>
          )}
          <Space size="large">
            <Button 
              type="primary" 
              icon={<SaveOutlined />}
              size="large"
              loading={saving}
              disabled={!hasUnsavedChanges}
              onClick={handleSave}
            >
              Save Changes
            </Button>
            <Button 
              icon={<UndoOutlined />}
              size="large"
              disabled={!hasUnsavedChanges || saving}
              onClick={handleRevert}
            >
              Revert Changes
            </Button>
          </Space>
          {!hasUnsavedChanges && (
            <Text type="secondary" style={{ display: 'block', marginTop: '8px', fontSize: '12px' }}>
              All changes saved ✓
            </Text>
          )}
        </Card>
        
        {/* Settings Preview Footer */}
        <Card style={{ marginTop: '16px', background: '#fafafa' }}>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            <strong>Page Settings:</strong> Color scheme: {colorScheme} • 
            Visible sections: {Object.entries(sectionVisibility).filter(([_, visible]) => visible).length}/5
          </Text>
        </Card>
      </div>
    </div>
  );
};

export default PersonalPage; 