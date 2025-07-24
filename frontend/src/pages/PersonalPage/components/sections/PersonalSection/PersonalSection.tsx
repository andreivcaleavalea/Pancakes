import React from 'react';
import { SECTION_COLORS } from '../../../constants';
import type { User, SectionSettings, TemplateOption } from '../../../types';
import {
  HeroTemplate,
  CreativeTemplate,
  ProfessionalTemplate,
  ModernTemplate,
  CardTemplate,
  MinimalTemplate,
} from './templates';

interface PersonalSectionProps {
  sectionKey: string;
  user: User;
  primaryColor: string;
  currentSectionSettings: SectionSettings;
  onSectionSettingsChange: (sectionKey: string, newSettings: SectionSettings) => void;
  templateOptions: TemplateOption[];
}

const PersonalSection: React.FC<PersonalSectionProps> = ({
  sectionKey,
  user,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  const { template, advancedSettings } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

  // Common props for all templates
  const templateProps = {
    user,
    sectionKey,
    sectionPrimaryColor,
    currentSectionSettings,
    onSectionSettingsChange,
    templateOptions,
    advancedSettings,
  };

  // Template selector
  const renderTemplate = () => {
    switch (template) {
      case 'hero':
        return <HeroTemplate {...templateProps} />;
      case 'creative':
        return <CreativeTemplate {...templateProps} />;
      case 'professional':
        return <ProfessionalTemplate {...templateProps} />;
      case 'modern':
        return <ModernTemplate {...templateProps} />;
      case 'minimal':
        return <MinimalTemplate {...templateProps} />;
      case 'card':
      default:
        return <CardTemplate {...templateProps} />;
    }
  };

  return <>{renderTemplate()}</>;
};

export default PersonalSection; 