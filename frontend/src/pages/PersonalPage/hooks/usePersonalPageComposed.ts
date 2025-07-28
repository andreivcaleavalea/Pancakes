import { useCallback } from 'react';
import { usePersonalPageData } from './usePersonalPageData';
import { usePersonalPageSettings } from './usePersonalPageSettings';
import { usePersonalPageActions } from './usePersonalPageActions';
import { SECTION_COLORS, ALL_TEMPLATE_OPTIONS } from '../constants';

/**
 * Main hook that orchestrates the PersonalPage functionality
 * Follows Composition over Inheritance - combines focused hooks
 */
export const usePersonalPage = () => {
  // Get data from specialized hooks
  const {
    user,
    educations,
    jobs,
    projects,
    hobbies,
    originalSettings,
    isLoading,
    error,
    refetchSettings,
  } = usePersonalPageData();

  const {
    draftSectionSettings,
    draftSectionOrder,
    draftSectionVisibility,
    draftColorScheme,
    draftIsPublic,
    draftPageSlug,
    hasUnsavedChanges,
    updateSectionSettings,
    updateSectionOrder,
    updateSectionVisibility,
    updateColorScheme,
    updatePublicToggle,
    updatePageSlug,
    resetToOriginal,
  } = usePersonalPageSettings(originalSettings || undefined);

  const { isSaving, saveSettings } = usePersonalPageActions(
    () => {
      // On success callback
      refetchSettings();
    },
    (error) => {
      // On error callback
      console.error('Save failed:', error);
    }
  );

  // Combined save handler
  const handleSave = useCallback(async () => {
    if (!hasUnsavedChanges || isSaving) return;

    const success = await saveSettings(
      draftSectionSettings,
      draftSectionOrder,
      draftSectionVisibility,
      draftColorScheme,
      draftIsPublic,
      draftPageSlug
    );

    return !!success;
  }, [
    hasUnsavedChanges,
    isSaving,
    saveSettings,
    draftSectionSettings,
    draftSectionOrder,
    draftSectionVisibility,
    draftColorScheme,
    draftIsPublic,
    draftPageSlug,
  ]);

  // Combined revert handler
  const handleRevert = useCallback(() => {
    resetToOriginal();
  }, [resetToOriginal]);

  // Get section data
  const getSectionData = useCallback((sectionKey: string) => {
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
        return null;
    }
  }, [user, educations, jobs, projects, hobbies]);

  // Check if section should render
  const shouldRenderSection = useCallback((sectionKey: string) => {
    const data = getSectionData(sectionKey);
    
    // Check visibility setting
    if (!draftSectionVisibility[sectionKey as keyof typeof draftSectionVisibility]) {
      return false;
    }

    // Check data availability
    switch (sectionKey) {
      case 'personal':
        return !!data;
      case 'education':
      case 'jobs':
      case 'projects':
      case 'hobbies':
        return Array.isArray(data) && data.length > 0;
      default:
        return false;
    }
  }, [getSectionData, draftSectionVisibility]);

  // Get section props
  const getSectionProps = useCallback((sectionKey: string) => {
    const currentSectionSettings = draftSectionSettings[sectionKey as keyof typeof draftSectionSettings];
    const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings?.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;
    const templateOptions = ALL_TEMPLATE_OPTIONS[sectionKey as keyof typeof ALL_TEMPLATE_OPTIONS] || [];
    
    return {
      sectionKey,
      user,
      primaryColor: '#1890ff',
      sectionPrimaryColor,
      currentSectionSettings,
      onSectionSettingsChange: updateSectionSettings,
      templateOptions,
      advancedSettings: currentSectionSettings?.advancedSettings,
      editMode: true,
    };
  }, [draftSectionSettings, user, updateSectionSettings]);

  // Count visible sections
  const visibleSectionCount = Object.values(draftSectionVisibility).filter(Boolean).length;

  return {
    // Data
    user,
    educations,
    jobs,
    projects,
    hobbies,
    isLoading,
    error,
    
    // Current settings
    sectionSettings: draftSectionSettings,
    sectionOrder: draftSectionOrder,
    sectionVisibility: draftSectionVisibility,
    colorScheme: draftColorScheme,
    isPublic: draftIsPublic,
    pageSlug: draftPageSlug,
    
    // Original settings for comparison
    originalSettings,
    
    // State
    hasUnsavedChanges,
    isSaving,
    
    // Actions
    handleSave,
    handleRevert,
    updateSectionSettings,
    updateSectionOrder,
    updateSectionVisibility,
    updateColorScheme,
    updatePublicToggle,
    updatePageSlug,
    
    // Helper functions
    getSectionData,
    shouldRenderSection,
    getSectionProps,
    
    // Computed values
    visibleSectionCount,
  };
};
