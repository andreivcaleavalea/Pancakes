import { useState, useEffect, useCallback } from 'react';
import type { 
  SectionSettings, 
  SectionSettingsMap, 
  SectionVisibility 
} from '../types';
import type { PersonalPageSettings } from '../../../services/personalPageService';
import { 
  DEFAULT_SECTION_ORDER, 
  DEFAULT_SECTION_VISIBILITY, 
  DEFAULT_SECTION_SETTINGS,
  DEFAULT_PUBLIC_SETTINGS 
} from '../constants';

/**
 * Hook responsible for managing personal page settings state
 * Follows Single Responsibility Principle - only handles settings state management
 */
export const usePersonalPageSettings = (originalSettings?: PersonalPageSettings) => {
  // Temporary state for editing (prefixed with 'draft' for clarity)
  const [draftSectionSettings, setDraftSectionSettings] = useState<SectionSettingsMap>(DEFAULT_SECTION_SETTINGS);
  const [draftSectionOrder, setDraftSectionOrder] = useState<string[]>(DEFAULT_SECTION_ORDER);
  const [draftSectionVisibility, setDraftSectionVisibility] = useState<SectionVisibility>(DEFAULT_SECTION_VISIBILITY);
  const [draftColorScheme, setDraftColorScheme] = useState<string>('blue');
  const [draftIsPublic, setDraftIsPublic] = useState<boolean>(DEFAULT_PUBLIC_SETTINGS.isPublic);
  const [draftPageSlug, setDraftPageSlug] = useState<string>(DEFAULT_PUBLIC_SETTINGS.pageSlug);
  
  // Change tracking
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);

  // Initialize draft settings when original settings are available
  useEffect(() => {
    if (!originalSettings) return;

    const updatedSectionSettings = { ...DEFAULT_SECTION_SETTINGS };
    
    // Update with settings from API
    Object.keys(updatedSectionSettings).forEach((key) => {
      const sectionKey = key as keyof SectionSettingsMap;
      updatedSectionSettings[sectionKey] = {
        template: originalSettings.sectionTemplates?.[sectionKey] || DEFAULT_SECTION_SETTINGS[sectionKey].template,
        color: originalSettings.sectionColors?.[sectionKey] || DEFAULT_SECTION_SETTINGS[sectionKey].color,
        advancedSettings: originalSettings.sectionAdvancedSettings?.[sectionKey],
      };
    });

    setDraftSectionSettings(updatedSectionSettings);
    setDraftSectionOrder(originalSettings.sectionOrder || DEFAULT_SECTION_ORDER);
    
    // Convert Record<string, boolean> to SectionVisibility
    const visibility: SectionVisibility = {
      personal: originalSettings.sectionVisibility?.personal ?? DEFAULT_SECTION_VISIBILITY.personal,
      education: originalSettings.sectionVisibility?.education ?? DEFAULT_SECTION_VISIBILITY.education,
      jobs: originalSettings.sectionVisibility?.jobs ?? DEFAULT_SECTION_VISIBILITY.jobs,
      projects: originalSettings.sectionVisibility?.projects ?? DEFAULT_SECTION_VISIBILITY.projects,
      hobbies: originalSettings.sectionVisibility?.hobbies ?? DEFAULT_SECTION_VISIBILITY.hobbies,
    };
    setDraftSectionVisibility(visibility);
    
    setDraftColorScheme(originalSettings.colorScheme || 'blue');
    setDraftIsPublic(originalSettings.isPublic ?? DEFAULT_PUBLIC_SETTINGS.isPublic);
    setDraftPageSlug(originalSettings.pageSlug || DEFAULT_PUBLIC_SETTINGS.pageSlug);
    setHasUnsavedChanges(false);
  }, [originalSettings]);

  // Check if current draft settings differ from original
  const checkForChanges = useCallback(() => {
    if (!originalSettings) return false;

    // Check section settings (templates and colors)
    const originalSectionSettings = { ...DEFAULT_SECTION_SETTINGS };
    Object.keys(originalSectionSettings).forEach((key) => {
      const sectionKey = key as keyof SectionSettingsMap;
      originalSectionSettings[sectionKey] = {
        template: originalSettings.sectionTemplates?.[sectionKey] || DEFAULT_SECTION_SETTINGS[sectionKey].template,
        color: originalSettings.sectionColors?.[sectionKey] || DEFAULT_SECTION_SETTINGS[sectionKey].color,
        advancedSettings: originalSettings.sectionAdvancedSettings?.[sectionKey] || DEFAULT_SECTION_SETTINGS[sectionKey].advancedSettings,
      };
    });

    const settingsChanged = JSON.stringify(draftSectionSettings) !== JSON.stringify(originalSectionSettings);
    const orderChanged = JSON.stringify(draftSectionOrder) !== JSON.stringify(originalSettings.sectionOrder || DEFAULT_SECTION_ORDER);
    
    // Convert original visibility to SectionVisibility for comparison
    const originalVisibility: SectionVisibility = {
      personal: originalSettings.sectionVisibility?.personal ?? DEFAULT_SECTION_VISIBILITY.personal,
      education: originalSettings.sectionVisibility?.education ?? DEFAULT_SECTION_VISIBILITY.education,
      jobs: originalSettings.sectionVisibility?.jobs ?? DEFAULT_SECTION_VISIBILITY.jobs,
      projects: originalSettings.sectionVisibility?.projects ?? DEFAULT_SECTION_VISIBILITY.projects,
      hobbies: originalSettings.sectionVisibility?.hobbies ?? DEFAULT_SECTION_VISIBILITY.hobbies,
    };
    
    const visibilityChanged = JSON.stringify(draftSectionVisibility) !== JSON.stringify(originalVisibility);
    const colorSchemeChanged = draftColorScheme !== (originalSettings.colorScheme || 'blue');
    const publicChanged = draftIsPublic !== (originalSettings.isPublic ?? DEFAULT_PUBLIC_SETTINGS.isPublic);
    const slugChanged = draftPageSlug !== (originalSettings.pageSlug || DEFAULT_PUBLIC_SETTINGS.pageSlug);

    return settingsChanged || orderChanged || visibilityChanged || colorSchemeChanged || publicChanged || slugChanged;
  }, [originalSettings, draftSectionSettings, draftSectionOrder, draftSectionVisibility, draftColorScheme, draftIsPublic, draftPageSlug]);

  // Update hasUnsavedChanges when draft settings change
  useEffect(() => {
    setHasUnsavedChanges(checkForChanges());
  }, [checkForChanges]);

  // Setting update handlers
  const updateSectionSettings = (sectionKey: string, newSettings: SectionSettings) => {
    setDraftSectionSettings(prev => ({
      ...prev,
      [sectionKey]: newSettings
    }));
  };

  const updateSectionOrder = (newOrder: string[]) => {
    setDraftSectionOrder(newOrder);
  };

  const updateSectionVisibility = (sectionKey: string, visible: boolean) => {
    setDraftSectionVisibility(prev => ({
      ...prev,
      [sectionKey]: visible
    }));
  };

  const updateColorScheme = (newColorScheme: string) => {
    setDraftColorScheme(newColorScheme);
  };

  const updatePublicToggle = (isPublic: boolean) => {
    setDraftIsPublic(isPublic);
  };

  const updatePageSlug = (pageSlug: string) => {
    setDraftPageSlug(pageSlug);
  };

  // Reset all draft settings to original
  const resetToOriginal = () => {
    if (!originalSettings) return;

    const originalSectionSettings = { ...DEFAULT_SECTION_SETTINGS };
    Object.keys(originalSectionSettings).forEach((key) => {
      const sectionKey = key as keyof SectionSettingsMap;
      originalSectionSettings[sectionKey] = {
        template: originalSettings.sectionTemplates?.[sectionKey] || DEFAULT_SECTION_SETTINGS[sectionKey].template,
        color: originalSettings.sectionColors?.[sectionKey] || DEFAULT_SECTION_SETTINGS[sectionKey].color,
        advancedSettings: originalSettings.sectionAdvancedSettings?.[sectionKey] || DEFAULT_SECTION_SETTINGS[sectionKey].advancedSettings,
      };
    });

    const originalVisibility: SectionVisibility = {
      personal: originalSettings.sectionVisibility?.personal ?? DEFAULT_SECTION_VISIBILITY.personal,
      education: originalSettings.sectionVisibility?.education ?? DEFAULT_SECTION_VISIBILITY.education,
      jobs: originalSettings.sectionVisibility?.jobs ?? DEFAULT_SECTION_VISIBILITY.jobs,
      projects: originalSettings.sectionVisibility?.projects ?? DEFAULT_SECTION_VISIBILITY.projects,
      hobbies: originalSettings.sectionVisibility?.hobbies ?? DEFAULT_SECTION_VISIBILITY.hobbies,
    };

    setDraftSectionSettings(originalSectionSettings);
    setDraftSectionOrder(originalSettings.sectionOrder || DEFAULT_SECTION_ORDER);
    setDraftSectionVisibility(originalVisibility);
    setDraftColorScheme(originalSettings.colorScheme || 'blue');
    setDraftIsPublic(originalSettings.isPublic ?? DEFAULT_PUBLIC_SETTINGS.isPublic);
    setDraftPageSlug(originalSettings.pageSlug || DEFAULT_PUBLIC_SETTINGS.pageSlug);
    setHasUnsavedChanges(false);
  };

  return {
    // Current draft settings
    draftSectionSettings,
    draftSectionOrder,
    draftSectionVisibility,
    draftColorScheme,
    draftIsPublic,
    draftPageSlug,
    
    // State indicators
    hasUnsavedChanges,
    
    // Update handlers
    updateSectionSettings,
    updateSectionOrder,
    updateSectionVisibility,
    updateColorScheme,
    updatePublicToggle,
    updatePageSlug,
    resetToOriginal,
  };
};
