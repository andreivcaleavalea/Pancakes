import React from 'react';
import type { SectionRendererProps } from '../../../types';
import { SECTION_COLORS } from '../../../constants';
import { 
  HeroTemplate, 
  CreativeTemplate, 
  ProfessionalTemplate, 
  ModernTemplate, 
  CardTemplate, 
  MinimalTemplate 
} from './templates';

interface PersonalSectionProps extends Omit<SectionRendererProps, 'data'> {
  user: any;
}

const PersonalSection: React.FC<PersonalSectionProps> = ({
  sectionKey,
  user,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  const { template } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

  // Common props for all templates
  const templateProps = {
    user,
    sectionKey,
    sectionPrimaryColor,
    currentSectionSettings,
    onSectionSettingsChange,
    templateOptions,
  };

  // Template selector
  switch (template) {
    case 'hero':
      return <HeroTemplate {...templateProps} />;
    case 'creative':
      return <CreativeTemplate {...templateProps} />;
    case 'professional':
      return <ProfessionalTemplate {...templateProps} />;
    case 'modern':
      return <ModernTemplate {...templateProps} />;
    case 'card':
      return <CardTemplate {...templateProps} />;
    case 'minimal':
      return <MinimalTemplate {...templateProps} />;
    default:
      return <CardTemplate {...templateProps} />; // fallback
  }
};

export default PersonalSection; 