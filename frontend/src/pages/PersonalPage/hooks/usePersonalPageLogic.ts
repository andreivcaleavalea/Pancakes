import { useState, useEffect } from 'react';
import { App } from 'antd';
import { useProfile } from '../../../hooks/useProfile';
import type { SectionSettings, SectionSettingsMap, SectionVisibility } from '../types';
import { 
  DEFAULT_SECTION_ORDER, 
  DEFAULT_SECTION_VISIBILITY, 
  DEFAULT_SECTION_SETTINGS,
  ALL_TEMPLATE_OPTIONS
} from '../constants';

export const usePersonalPageLogic = (settings: any) => {
  const { message } = App.useApp();
  const { profileData, loading, error } = useProfile();

  const [sectionSettings, setSectionSettings] = useState<SectionSettingsMap>(DEFAULT_SECTION_SETTINGS);
  const [liveSettings, setLiveSettings] = useState<any>(null);

  // Extract data from profileData
  const user = profileData?.user || null;
  const educations = profileData?.educations || [];
  const jobs = profileData?.jobs || [];
  const projects = profileData?.projects || [];
  const hobbies = profileData?.hobbies || [];

  // Derived state
  const sectionOrder = settings?.sectionOrder || DEFAULT_SECTION_ORDER;
  const sectionVisibility: SectionVisibility = settings?.sectionVisibility || DEFAULT_SECTION_VISIBILITY;
  const primaryColor = '#1890ff'; // Could be made configurable
  const colorScheme = settings?.colorScheme || 'blue';

  // Initialize section settings from props
  useEffect(() => {
    if (settings) {
      const updatedSectionSettings = { ...DEFAULT_SECTION_SETTINGS };
      
      // Update with settings from props
      Object.keys(updatedSectionSettings).forEach((key) => {
        const sectionKey = key as keyof SectionSettingsMap;
        updatedSectionSettings[sectionKey] = {
          template: settings.sectionTemplates?.[sectionKey] || DEFAULT_SECTION_SETTINGS[sectionKey].template,
          color: settings.sectionColors?.[sectionKey] || DEFAULT_SECTION_SETTINGS[sectionKey].color,
        };
      });

      setSectionSettings(updatedSectionSettings);
    }
  }, [settings]);

  const handleSectionSettingsChange = (sectionKey: string, newSettings: SectionSettings) => {
    setSectionSettings(prev => ({
      ...prev,
      [sectionKey]: newSettings
    }));

    // Update live settings for preview
    setLiveSettings((prev: any) => ({
      ...prev,
      sectionTemplates: {
        ...prev?.sectionTemplates,
        [sectionKey]: newSettings.template
      },
      sectionColors: {
        ...prev?.sectionColors,
        [sectionKey]: newSettings.color
      }
    }));

    message.success(`${sectionKey.charAt(0).toUpperCase() + sectionKey.slice(1)} section updated!`);
  };

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

  const getSectionProps = (sectionKey: string) => {
    const currentSectionSettings = sectionSettings[sectionKey as keyof SectionSettingsMap] || DEFAULT_SECTION_SETTINGS[sectionKey as keyof SectionSettingsMap];
    const templateOptions = ALL_TEMPLATE_OPTIONS[sectionKey as keyof typeof ALL_TEMPLATE_OPTIONS] || [];

    return {
      sectionKey,
      user,
      data: getSectionData(sectionKey),
      primaryColor,
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
    
    // Settings
    sectionSettings,
    liveSettings,
    sectionOrder,
    sectionVisibility,
    primaryColor,
    colorScheme,
    
    // Functions
    handleSectionSettingsChange,
    getSectionData,
    getSectionProps,
  };
}; 