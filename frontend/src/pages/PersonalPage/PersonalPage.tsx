import React from 'react';
import { Card, Typography, Spin, Alert } from 'antd';
import { PersonalSection, EducationSection, JobsSection, ProjectsSection, HobbiesSection } from './components/sections';
import { usePersonalPageLogic } from './hooks/usePersonalPageLogic';
import type { SectionVisibility } from './types';
import './PersonalPage.scss';

const { Text } = Typography;

interface PersonalPageProps {
  settings?: any;
  sectionSettings?: any;
  onSectionSettingsChange?: (sectionKey: string, newSettings: any) => void;
}

// Section renderer mapping
const SECTION_COMPONENTS = {
  personal: PersonalSection,
  education: EducationSection,
  jobs: JobsSection,
  projects: ProjectsSection,
  hobbies: HobbiesSection,
} as const;

const PersonalPage: React.FC<PersonalPageProps> = ({
  settings,
  sectionSettings: externalSectionSettings,
  onSectionSettingsChange: externalOnSectionSettingsChange,
}) => {
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
    getSectionProps,
  } = usePersonalPageLogic(settings);

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

    // Override with external handlers if provided
    if (externalOnSectionSettingsChange) {
      sectionProps.onSectionSettingsChange = externalOnSectionSettingsChange;
    }

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
        (sectionSpecificProps as any).educations = data;
        break;
      case 'jobs':
        (sectionSpecificProps as any).jobs = data;
        break;
      case 'projects':
        (sectionSpecificProps as any).projects = data;
        break;
      case 'hobbies':
        (sectionSpecificProps as any).hobbies = data;
        break;
    }

    return (
      <SectionComponent
        key={sectionKey}
        {...sectionSpecificProps}
        data={data}
      />
    );
  };

  return (
    <div className="personal-page" style={{ padding: '24px' }}>
      <div style={{ width: '100%' }}>
        {/* Render sections in order */}
        {sectionOrder.map((sectionKey: string) => renderSection(sectionKey))}
        
        {/* Settings Preview Footer */}
        {settings && (
          <Card style={{ marginTop: '24px', background: '#fafafa' }}>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              <strong>Page Settings:</strong> {settings.pageSlug ? `pancakes.dev/personal/${settings.pageSlug}` : 'URL not set'} • 
              Color: {colorScheme} • 
              Visible sections: {Object.entries(sectionVisibility).filter(([_, visible]) => visible).length}/5
            </Text>
          </Card>
        )}
      </div>
    </div>
  );
};

export default PersonalPage; 