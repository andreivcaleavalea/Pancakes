import React from 'react';
import { usePersonalPageComposed } from './hooks';
import { PersonalPageSections } from './components/PersonalPageSections';
import SectionOrderControl from './components/SectionOrderControl';
import PublicSettingsControl from './components/PublicSettingsControl';
import { LoadingState } from './components/ui/LoadingState';
import { ErrorState } from './components/ui/ErrorState';
import { PersonalPageActions } from './components/ui/PersonalPageActions';
import { PersonalPageFooter } from './components/ui/PersonalPageFooter';
import './PersonalPage.scss';


const PersonalPage: React.FC = () => {
  const {
    user,
    // Data
    educations,
    jobs,
    projects,
    hobbies,
    isLoading,
    error,
    
    // Settings
    sectionSettings,
    sectionOrder,
    sectionVisibility,
    colorScheme,
    visibleSectionCount,
    isPublic,
    pageSlug,
    
    // State
    hasUnsavedChanges,
    isSaving,
    
    // Actions
    handleSave,
    handleRevert,
    updateSectionOrder,
    updateSectionVisibility,
    updateSectionSettings,
    updatePublicToggle,
    updatePageSlug,
  } = usePersonalPageComposed();

  // Loading state
  if (isLoading) {
    return <LoadingState />;
  }

  // Error state
  if (error) {
    return <ErrorState error={error} />;
  }

  // No profile data
  if (!isLoading && !user) {
    return (
      <ErrorState 
        error="Please create your profile first." 
        title="No profile data found"
        type="warning"
      />
    );
  }

  return (
    <div className="personal-page" style={{ padding: '24px' }}>
      <div style={{ width: '100%' }}>
        {/* Section Order Control */}
        <SectionOrderControl
          sectionOrder={sectionOrder}
          onSectionOrderChange={updateSectionOrder}
          sectionVisibility={{
            personal: sectionVisibility.personal,
            education: sectionVisibility.education,
            jobs: sectionVisibility.jobs,
            projects: sectionVisibility.projects,
            hobbies: sectionVisibility.hobbies,
          }}
          onSectionVisibilityChange={updateSectionVisibility}
        />
        
        {/* Public/Private Settings */}
        <PublicSettingsControl
          isPublic={isPublic}
          pageSlug={pageSlug}
          onPublicToggle={updatePublicToggle}
          onPageSlugChange={updatePageSlug}
        />
        
        {/* Render all sections */}
        <PersonalPageSections
          sectionOrder={sectionOrder}
          sectionVisibility={sectionVisibility}
          sectionSettings={sectionSettings}
          user={user}
          educations={educations}
          jobs={jobs}
          projects={projects}
          hobbies={hobbies}
          onSectionSettingsChange={updateSectionSettings}
        />
        
        {/* Action buttons */}
        <PersonalPageActions
          hasUnsavedChanges={hasUnsavedChanges}
          isSaving={isSaving}
          onSave={handleSave}
          onRevert={handleRevert}
        />
        
        {/* Settings footer */}
        <PersonalPageFooter
          colorScheme={colorScheme}
          visibleSectionCount={visibleSectionCount}
        />
      </div>
    </div>
  );
};

export default PersonalPage; 