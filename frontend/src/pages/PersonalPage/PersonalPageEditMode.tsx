import React, { useState } from 'react';
import { Card, Typography, Spin, Alert, Button, Space, Switch, Input, message, Tooltip } from 'antd';
import { SaveOutlined, UndoOutlined, GlobalOutlined, CopyOutlined, ReloadOutlined } from '@ant-design/icons';
import { PersonalSection, EducationSection, JobsSection, ProjectsSection, HobbiesSection } from './components/sections';
import SectionOrderControl from './components/SectionOrderControl';
import { usePersonalPageEditMode } from './hooks/usePersonalPageEditMode';
import { PersonalPageService } from '../../services/personalPageService';
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
    isPublic,
    pageSlug,
    hasUnsavedChanges,
    saving,
    handleSave,
    handleRevert,
    handleSectionOrderChange,
    handleSectionVisibilityChange,
    handlePublicToggleChange,
    handlePageSlugChange,
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

        {/* Public Settings Control */}
        <Card 
          className="public-settings-control"
          title={
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <GlobalOutlined />
              <span>Public Blog Settings</span>
            </div>
          }
          size="small"
          style={{ marginBottom: '16px' }}
        >
          <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
            {/* Public Toggle */}
            <div className="public-toggle-section" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <Text strong>Make blog public</Text>
                <div>
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    Allow anyone with the URL to view your personal page
                  </Text>
                </div>
              </div>
              <Switch 
                checked={isPublic}
                onChange={handlePublicToggleChange}
                checkedChildren="Public"
                unCheckedChildren="Private"
              />
            </div>

            {/* Slug Input */}
            {isPublic && (
              <div className="slug-input-section">
                <Text strong style={{ display: 'block', marginBottom: '8px' }}>
                  Custom URL Slug
                </Text>
                <div style={{ display: 'flex', gap: '8px', alignItems: 'flex-end' }}>
                  <div style={{ flex: 1 }}>
                    <Input
                      placeholder="my-awesome-blog"
                      value={pageSlug}
                      onChange={(e) => handlePageSlugChange(e.target.value)}
                      addonBefore="/public/"
                      style={{ width: '100%' }}
                    />
                    <Text type="secondary" style={{ fontSize: '11px', marginTop: '4px', display: 'block' }}>
                      Only letters, numbers, hyphens, and underscores allowed
                    </Text>
                  </div>
                  <Tooltip title="Generate random slug">
                    <Button 
                      icon={<ReloadOutlined />}
                      onClick={async () => {
                        try {
                          console.log('Generating slug for user:', user?.name || 'user'); // Debug logging
                          const newSlug = await PersonalPageService.generateSlug(user?.name || 'user');
                          console.log('Generated slug:', newSlug); // Debug logging
                          handlePageSlugChange(newSlug);
                          message.success('Generated new slug!');
                        } catch (error) {
                          console.error('Error generating slug:', error); // Debug logging
                          message.error(`Failed to generate slug: ${error}`);
                        }
                      }}
                    />
                  </Tooltip>
                  {pageSlug && (
                    <Tooltip title="Copy public URL">
                      <Button 
                        icon={<CopyOutlined />}
                        onClick={() => {
                          const url = `${window.location.origin}/public/${pageSlug}`;
                          navigator.clipboard.writeText(url);
                          message.success('URL copied to clipboard!');
                        }}
                      />
                    </Tooltip>
                  )}
                </div>
                {pageSlug && (
                  <div style={{ marginTop: '8px', padding: '8px', backgroundColor: '#f6f6f6', borderRadius: '4px' }}>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Public URL: <Text code>{window.location.origin}/public/{pageSlug}</Text>
                    </Text>
                  </div>
                )}
              </div>
            )}
          </div>
        </Card>
        
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
            <div style={{ marginBottom: '16px' }}>
              <Text type="warning" style={{ fontWeight: 500, fontSize: '14px' }}>
                ⚠️ You have unsaved changes
              </Text>
            </div>
          )}
          <Space size="large" wrap style={{ justifyContent: 'center', width: '100%' }}>
            <Button 
              type="primary" 
              icon={<SaveOutlined />}
              size="large"
              loading={saving}
              disabled={!hasUnsavedChanges}
              onClick={handleSave}
              className="save-btn"
            >
              <span className="btn-text">Save Changes</span>
            </Button>
            <Button 
              icon={<UndoOutlined />}
              size="large"
              disabled={!hasUnsavedChanges || saving}
              onClick={handleRevert}
              className="revert-btn"
            >
              <span className="btn-text">Revert Changes</span>
            </Button>
          </Space>
          {!hasUnsavedChanges && (
            <div style={{ marginTop: '12px' }}>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                All changes saved ✓
              </Text>
            </div>
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