import { useState, useEffect, useCallback } from 'react';
import { App } from 'antd';
import { useProfile } from '../../../hooks/useProfile';
import { usePersonalPage } from '../../../hooks/usePersonalPage';
import { PersonalPageService } from '../../../services/personalPageService';
import type { SectionSettings, SectionSettingsMap, SectionVisibility } from '../types';
import { 
  DEFAULT_SECTION_ORDER, 
  DEFAULT_SECTION_VISIBILITY, 
  DEFAULT_SECTION_SETTINGS,
  DEFAULT_PUBLIC_SETTINGS,
  ALL_TEMPLATE_OPTIONS
} from '../constants';

export const usePersonalPageEditMode = () => {
  const { message } = App.useApp();
  const { profileData, loading: profileLoading, error: profileError } = useProfile();
  const { settings: originalSettings, loading: settingsLoading, error: settingsError, refetch } = usePersonalPage();

  // Temporary state for editing
  const [tempSectionSettings, setTempSectionSettings] = useState<SectionSettingsMap>(DEFAULT_SECTION_SETTINGS);
  const [tempSectionOrder, setTempSectionOrder] = useState<string[]>(DEFAULT_SECTION_ORDER);
  const [tempSectionVisibility, setTempSectionVisibility] = useState<SectionVisibility>(DEFAULT_SECTION_VISIBILITY);
  const [tempColorScheme, setTempColorScheme] = useState<string>('blue');
  const [tempIsPublic, setTempIsPublic] = useState<boolean>(DEFAULT_PUBLIC_SETTINGS.isPublic);
  const [tempPageSlug, setTempPageSlug] = useState<string>(DEFAULT_PUBLIC_SETTINGS.pageSlug);
  
  // State management
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  const [saving, setSaving] = useState(false);

  // Extract data from profileData
  const user = profileData?.user || null;
  const educations = profileData?.educations || [];
  const jobs = profileData?.jobs || [];
  const projects = profileData?.projects || [];
  const hobbies = profileData?.hobbies || [];

  // Loading and error states
  const loading = profileLoading || settingsLoading;
  const error = profileError || settingsError;

  // Initialize temporary settings when original settings load
  useEffect(() => {
    if (originalSettings) {
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

      setTempSectionSettings(updatedSectionSettings);
      setTempSectionOrder(originalSettings.sectionOrder || DEFAULT_SECTION_ORDER);
      
      // Convert Record<string, boolean> to SectionVisibility
      const visibility: SectionVisibility = {
        personal: originalSettings.sectionVisibility?.personal ?? DEFAULT_SECTION_VISIBILITY.personal,
        education: originalSettings.sectionVisibility?.education ?? DEFAULT_SECTION_VISIBILITY.education,
        jobs: originalSettings.sectionVisibility?.jobs ?? DEFAULT_SECTION_VISIBILITY.jobs,
        projects: originalSettings.sectionVisibility?.projects ?? DEFAULT_SECTION_VISIBILITY.projects,
        hobbies: originalSettings.sectionVisibility?.hobbies ?? DEFAULT_SECTION_VISIBILITY.hobbies,
      };
      setTempSectionVisibility(visibility);
      
      setTempColorScheme(originalSettings.colorScheme || 'blue');
      setTempIsPublic(originalSettings.isPublic ?? DEFAULT_PUBLIC_SETTINGS.isPublic);
      setTempPageSlug(originalSettings.pageSlug || DEFAULT_PUBLIC_SETTINGS.pageSlug);
      setHasUnsavedChanges(false);
    }
  }, [originalSettings]);

  // Check if current temp settings differ from original
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

    const settingsChanged = JSON.stringify(tempSectionSettings) !== JSON.stringify(originalSectionSettings);
    const orderChanged = JSON.stringify(tempSectionOrder) !== JSON.stringify(originalSettings.sectionOrder || DEFAULT_SECTION_ORDER);
    
    // Convert original visibility to SectionVisibility for comparison
    const originalVisibility: SectionVisibility = {
      personal: originalSettings.sectionVisibility?.personal ?? DEFAULT_SECTION_VISIBILITY.personal,
      education: originalSettings.sectionVisibility?.education ?? DEFAULT_SECTION_VISIBILITY.education,
      jobs: originalSettings.sectionVisibility?.jobs ?? DEFAULT_SECTION_VISIBILITY.jobs,
      projects: originalSettings.sectionVisibility?.projects ?? DEFAULT_SECTION_VISIBILITY.projects,
      hobbies: originalSettings.sectionVisibility?.hobbies ?? DEFAULT_SECTION_VISIBILITY.hobbies,
    };
    const visibilityChanged = JSON.stringify(tempSectionVisibility) !== JSON.stringify(originalVisibility);
    const colorSchemeChanged = tempColorScheme !== (originalSettings.colorScheme || 'blue');
    const publicChanged = tempIsPublic !== (originalSettings.isPublic ?? DEFAULT_PUBLIC_SETTINGS.isPublic);
    const slugChanged = tempPageSlug !== (originalSettings.pageSlug || DEFAULT_PUBLIC_SETTINGS.pageSlug);

    return settingsChanged || orderChanged || visibilityChanged || colorSchemeChanged || publicChanged || slugChanged;
  }, [originalSettings, tempSectionSettings, tempSectionOrder, tempSectionVisibility, tempColorScheme, tempIsPublic, tempPageSlug]);

  // Update hasUnsavedChanges when temp settings change
  useEffect(() => {
    setHasUnsavedChanges(checkForChanges());
  }, [checkForChanges]);

  // Handle section settings change (temporary)
  const handleSectionSettingsChange = (sectionKey: string, newSettings: SectionSettings) => {
    setTempSectionSettings(prev => ({
      ...prev,
      [sectionKey]: newSettings
    }));
  };

  // Handle section order change
  const handleSectionOrderChange = (newOrder: string[]) => {
    setTempSectionOrder(newOrder);
  };

  // Handle section visibility change
  const handleSectionVisibilityChange = (sectionKey: string, visible: boolean) => {
    setTempSectionVisibility(prev => ({
      ...prev,
      [sectionKey]: visible
    }));
  };

  // Handle color scheme change
  const handleColorSchemeChange = (newColorScheme: string) => {
    setTempColorScheme(newColorScheme);
  };

  // Handle public toggle change
  const handlePublicToggleChange = (isPublic: boolean) => {
    setTempIsPublic(isPublic);
  };

  // Handle page slug change
  const handlePageSlugChange = (pageSlug: string) => {
    setTempPageSlug(pageSlug);
  };

  // Save all changes to database
  const handleSave = async () => {
    if (!hasUnsavedChanges || saving) return;

    try {
      setSaving(true);

      // Helper function to sanitize advanced settings for backend
      const sanitizeAdvancedSettings = (advancedSettings: any) => {
        if (!advancedSettings) return null;
        
        return {
          layout: {
            fullscreen: Boolean(advancedSettings.layout?.fullscreen),
            margin: advancedSettings.layout?.margin || 16
          },
          background: {
            color: advancedSettings.background?.color || "",
            pattern: advancedSettings.background?.pattern || "none",
            opacity: advancedSettings.background?.opacity || 1.0
          },
          typography: {
            fontSize: advancedSettings.typography?.fontSize || "medium",
            fontColor: advancedSettings.typography?.fontColor || "",
            fontWeight: advancedSettings.typography?.fontWeight || "normal"
          },
          styling: {
            roundCorners: Boolean(advancedSettings.styling?.roundCorners),
            borderRadius: advancedSettings.styling?.borderRadius || "8px",
            shadow: Boolean(advancedSettings.styling?.shadow),
            shadowIntensity: advancedSettings.styling?.shadowIntensity || "medium",
            border: {
              enabled: Boolean(advancedSettings.styling?.border?.enabled),
              color: advancedSettings.styling?.border?.color || "#e5e7eb",
              width: advancedSettings.styling?.border?.width || "1px",
              style: advancedSettings.styling?.border?.style || "solid"
            }
          }
        };
      };

      // Prepare section templates for API
      const sectionTemplates: Record<string, string> = {};
      const sectionColors: Record<string, string> = {};
      const sectionAdvancedSettings: Record<string, any> = {};
      
      Object.entries(tempSectionSettings).forEach(([key, settings]) => {
        sectionTemplates[key] = settings.template;
        sectionColors[key] = settings.color;
        
        // Only include advanced settings if they exist and are properly sanitized
        const sanitized = sanitizeAdvancedSettings(settings.advancedSettings);
        if (sanitized) {
          sectionAdvancedSettings[key] = sanitized;
        }
      });

      // Convert SectionVisibility to Record<string, boolean>
      const sectionVisibility: Record<string, boolean> = {
        personal: tempSectionVisibility.personal,
        education: tempSectionVisibility.education,
        jobs: tempSectionVisibility.jobs,
        projects: tempSectionVisibility.projects,
        hobbies: tempSectionVisibility.hobbies,
      };

      // Call API to update settings
      await PersonalPageService.updateSettings({
        isPublic: tempIsPublic,
        pageSlug: tempPageSlug,
        sectionOrder: tempSectionOrder,
        sectionVisibility,
        sectionTemplates,
        sectionColors,
        sectionAdvancedSettings,
        colorScheme: tempColorScheme,
      });

      // Refetch original settings to sync
      await refetch();
      
      setHasUnsavedChanges(false);
      message.success('Personal page settings saved successfully!');
    } catch (err) {
      message.error('Failed to save settings. Please try again.');
      console.error('Save error:', err);
    } finally {
      setSaving(false);
    }
  };

  // Revert all changes
  const handleRevert = () => {
    if (!originalSettings) return;

    // Reset to original settings
    const originalSectionSettings = { ...DEFAULT_SECTION_SETTINGS };
    Object.keys(originalSectionSettings).forEach((key) => {
      const sectionKey = key as keyof SectionSettingsMap;
      originalSectionSettings[sectionKey] = {
        template: originalSettings.sectionTemplates?.[sectionKey] || DEFAULT_SECTION_SETTINGS[sectionKey].template,
        color: originalSettings.sectionColors?.[sectionKey] || DEFAULT_SECTION_SETTINGS[sectionKey].color,
        advancedSettings: originalSettings.sectionAdvancedSettings?.[sectionKey] || DEFAULT_SECTION_SETTINGS[sectionKey].advancedSettings,
      };
    });

    // Convert original visibility to SectionVisibility
    const originalVisibility: SectionVisibility = {
      personal: originalSettings.sectionVisibility?.personal ?? DEFAULT_SECTION_VISIBILITY.personal,
      education: originalSettings.sectionVisibility?.education ?? DEFAULT_SECTION_VISIBILITY.education,
      jobs: originalSettings.sectionVisibility?.jobs ?? DEFAULT_SECTION_VISIBILITY.jobs,
      projects: originalSettings.sectionVisibility?.projects ?? DEFAULT_SECTION_VISIBILITY.projects,
      hobbies: originalSettings.sectionVisibility?.hobbies ?? DEFAULT_SECTION_VISIBILITY.hobbies,
    };

    setTempSectionSettings(originalSectionSettings);
    setTempSectionOrder(originalSettings.sectionOrder || DEFAULT_SECTION_ORDER);
    setTempSectionVisibility(originalVisibility);
    setTempColorScheme(originalSettings.colorScheme || 'blue');
    setTempIsPublic(originalSettings.isPublic ?? DEFAULT_PUBLIC_SETTINGS.isPublic);
    setTempPageSlug(originalSettings.pageSlug || DEFAULT_PUBLIC_SETTINGS.pageSlug);
    setHasUnsavedChanges(false);
    
    message.info('All changes reverted to last saved state');
  };

  // Get section data
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

  // Get section props for rendering
  const getSectionProps = (sectionKey: string) => {
    const currentSectionSettings = tempSectionSettings[sectionKey as keyof SectionSettingsMap] || DEFAULT_SECTION_SETTINGS[sectionKey as keyof SectionSettingsMap];
    const templateOptions = ALL_TEMPLATE_OPTIONS[sectionKey as keyof typeof ALL_TEMPLATE_OPTIONS] || [];

    return {
      sectionKey,
      user,
      data: getSectionData(sectionKey),
      primaryColor: '#1890ff',
      currentSectionSettings,
      onSectionSettingsChange: handleSectionSettingsChange,
      templateOptions,
    };
  };

  return {
    // Data
    user,
    educations,
    jobs,
    projects,
    hobbies,
    loading,
    error,
    
    // Current temporary settings
    sectionSettings: tempSectionSettings,
    sectionOrder: tempSectionOrder,
    sectionVisibility: tempSectionVisibility,
    colorScheme: tempColorScheme,
    isPublic: tempIsPublic,
    pageSlug: tempPageSlug,
    
    // Original settings for comparison
    originalSettings,
    
    // State
    hasUnsavedChanges,
    saving,
    
    // Functions
    handleSectionSettingsChange,
    handleSectionOrderChange,
    handleSectionVisibilityChange,
    handleColorSchemeChange,
    handlePublicToggleChange,
    handlePageSlugChange,
    handleSave,
    handleRevert,
    getSectionData,
    getSectionProps,
  };
}; 