import React from 'react';
import { Card, Typography, Spin, Alert, Button, Space } from 'antd';
import { SaveOutlined, UndoOutlined } from '@ant-design/icons';
import { PersonalSection, EducationSection, JobsSection, ProjectsSection, HobbiesSection } from './components/sections';
import { usePersonalPageEditMode } from './hooks/usePersonalPageEditMode';
import type { SectionVisibility } from './types';
import './PersonalPage.scss';

const { Text } = Typography;

// Section renderer mapping
const SECTION_COMPONENTS = {
  personal: PersonalSection,
  education: EducationSection,
  jobs: JobsSection,
  projects: ProjectsSection,
  hobbies: HobbiesSection,
} as const;

const PersonalPageEditMode: React.FC = () => {
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

    // Get the appropriate component
    const SectionComponent = SECTION_COMPONENTS[sectionKey as keyof typeof SECTION_COMPONENTS];
    if (!SectionComponent) {
      return null;
    }

    // Get section props from hook
    const sectionProps = getSectionProps(sectionKey);

    // Get section-specific data
    let data: any = null;
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
    if (sectionKey === 'personal') {
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
    
    // Add specific data props for each section type
    switch (sectionKey) {
      case 'education':
        return (
          <EducationSection
            key={sectionKey}
            {...sectionSpecificProps}
            educations={data}
          />
        );
      case 'jobs':
        return (
          <JobsSection
            key={sectionKey}
            {...sectionSpecificProps}
            jobs={data}
          />
        );
      case 'projects':
        return (
          <ProjectsSection
            key={sectionKey}
            {...sectionSpecificProps}
            projects={data}
          />
        );
      case 'hobbies':
        return (
          <HobbiesSection
            key={sectionKey}
            {...sectionSpecificProps}
            hobbies={data}
          />
        );
      default:
        return null;
    }
  };

  return (
    <div className="personal-page" style={{ padding: '24px' }}>
      <div style={{ width: '100%' }}>
        {/* Render sections in order */}
        {sectionOrder.map((sectionKey: string) => renderSection(sectionKey))}
        
        {/* Save/Revert Action Buttons */}
        <Card 
          className={`personal-page__action-buttons ${hasUnsavedChanges ? 'has-changes' : ''}`}
          style={{ 
            marginTop: '32px', 
            textAlign: 'center', 
            background: hasUnsavedChanges ? '#fff7e6' : '#fafafa', 
            borderColor: hasUnsavedChanges ? '#ffa940' : '#d9d9d9' 
          }}
        >
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
        <Card className="personal-page__settings-footer" style={{ marginTop: '16px', background: '#fafafa' }}>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            <strong>Page Settings:</strong> Color scheme: {colorScheme} • 
            Visible sections: {Object.entries(sectionVisibility).filter(([_, visible]) => visible).length}/5
          </Text>
        </Card>
      </div>
    </div>
  );
};

export default PersonalPageEditMode; 