import React from 'react';
import type { Hobby, SectionSettings, TemplateOption } from '../../../types';
import { SECTION_COLORS } from '../../../constants';
import { 
  ColorfulTemplate, 
  CreativeTemplate, 
  IconsTemplate, 
  InterestsTemplate, 
  MinimalTemplate, 
  TagsTemplate 
} from './templates';

interface HobbiesSectionProps {
  sectionKey: string;
  hobbies: Hobby[];
  primaryColor: string;
  currentSectionSettings: SectionSettings;
  onSectionSettingsChange: (sectionKey: string, newSettings: SectionSettings) => void;
  templateOptions: TemplateOption[];
}

const HobbiesSection: React.FC<HobbiesSectionProps> = ({
  sectionKey,
  hobbies,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  const { template, advancedSettings } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

  // Common props for all templates
  const templateProps = {
    hobbies,
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
      case 'colorful':
        return <ColorfulTemplate {...templateProps} />;
      case 'creative':
        return <CreativeTemplate {...templateProps} />;
      case 'icons':
        return <IconsTemplate {...templateProps} />;
      case 'interests':
        return <InterestsTemplate {...templateProps} />;
      case 'minimal':
        return <MinimalTemplate {...templateProps} />;
      case 'tags':
      default:
        return <TagsTemplate {...templateProps} />;
    }
  };

  return <>{renderTemplate()}</>;
};

export default HobbiesSection; 