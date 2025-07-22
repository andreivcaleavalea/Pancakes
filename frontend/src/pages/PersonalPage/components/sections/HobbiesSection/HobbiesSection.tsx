import React from 'react';
import type { SectionRendererProps } from '../../../types';
import { SECTION_COLORS } from '../../../constants';
import { 
  TagsTemplate, 
  CreativeTemplate, 
  MinimalTemplate, 
  InterestsTemplate, 
  ColorfulTemplate, 
  IconsTemplate 
} from './templates';

interface HobbiesSectionProps extends Omit<SectionRendererProps, 'data'> {
  hobbies: any[];
}

const HobbiesSection: React.FC<HobbiesSectionProps> = ({
  sectionKey,
  hobbies,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  const { template } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

  // Common props for all templates
  const templateProps = {
    hobbies,
    sectionKey,
    sectionPrimaryColor,
    currentSectionSettings,
    onSectionSettingsChange,
    templateOptions,
  };

  // Template selector
  switch (template) {
    case 'tags':
      return <TagsTemplate {...templateProps} />;
    case 'creative':
      return <CreativeTemplate {...templateProps} />;
    case 'minimal':
      return <MinimalTemplate {...templateProps} />;
    case 'interests':
      return <InterestsTemplate {...templateProps} />;
    case 'colorful':
      return <ColorfulTemplate {...templateProps} />;
    case 'icons':
      return <IconsTemplate {...templateProps} />;
    default:
      return <TagsTemplate {...templateProps} />; // fallback
  }
};

export default HobbiesSection; 