import { useState } from 'react';
import { App } from 'antd';
import { PersonalPageService } from '../../../services/personalPageService';
import type { 
  SectionSettingsMap, 
  SectionVisibility 
} from '../types';
import type { PersonalPageSettings } from '../../../services/personalPageService';

/**
 * Hook responsible for handling save/revert actions for personal page
 */
export const usePersonalPageActions = (
  onSuccess?: () => void,
  onError?: (error: string) => void
) => {
  const { message } = App.useApp();
  const [isSaving, setIsSaving] = useState(false);


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

  /**
   * Saves all personal page settings to the backend
   */
  const saveSettings = async (
    draftSectionSettings: SectionSettingsMap,
    draftSectionOrder: string[],
    draftSectionVisibility: SectionVisibility,
    draftColorScheme: string,
    draftIsPublic: boolean,
    draftPageSlug: string
  ): Promise<PersonalPageSettings | null> => {
    if (isSaving) return null;

    try {
      setIsSaving(true);

      // Prepare section templates for API
      const sectionTemplates: Record<string, string> = {};
      const sectionColors: Record<string, string> = {};
      const sectionAdvancedSettings: Record<string, any> = {};
      
      Object.entries(draftSectionSettings).forEach(([key, settings]) => {
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
        personal: draftSectionVisibility.personal,
        education: draftSectionVisibility.education,
        jobs: draftSectionVisibility.jobs,
        projects: draftSectionVisibility.projects,
        hobbies: draftSectionVisibility.hobbies,
      };

      // Call API to update settings
      const updatedSettings = await PersonalPageService.updateSettings({
        isPublic: draftIsPublic,
        pageSlug: draftPageSlug,
        sectionOrder: draftSectionOrder,
        sectionVisibility,
        sectionTemplates,
        sectionColors,
        sectionAdvancedSettings,
        colorScheme: draftColorScheme,
      });

      message.success('Personal page settings saved successfully!');
      onSuccess?.();
      
      return updatedSettings;
    } catch (err) {
      const errorMessage = 'Failed to save settings. Please try again.';
      message.error(errorMessage);
      onError?.(errorMessage);
      console.error('Save error:', err);
      return null;
    } finally {
      setIsSaving(false);
    }
  };

  return {
    isSaving,
    saveSettings,
  };
};
