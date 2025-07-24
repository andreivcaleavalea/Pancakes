import React from 'react';
import type { Education, SectionSettings, TemplateOption } from '../../../types';
import { SECTION_COLORS } from '../../../constants';
import { 
  AcademicTemplate, 
  TimelineTemplate, 
  GridTemplate, 
  JourneyTemplate, 
  UniversityTemplate, 
  ProgressTemplate 
} from './templates';

interface EducationSectionProps {
  sectionKey: string;
  educations: Education[];
  primaryColor: string;
  currentSectionSettings: SectionSettings;
  onSectionSettingsChange: (sectionKey: string, newSettings: SectionSettings) => void;
  templateOptions: TemplateOption[];
}

const EducationSection: React.FC<EducationSectionProps> = ({
  sectionKey,
  educations,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  const { template, advancedSettings } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

  // Common props for all templates
  const templateProps = {
    educations,
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
      case 'academic':
        return <AcademicTemplate {...templateProps} />;
      case 'timeline':
        return <TimelineTemplate {...templateProps} />;
      case 'grid':
        return <GridTemplate {...templateProps} />;
      case 'journey':
        return <JourneyTemplate {...templateProps} />;
      case 'university':
        return <UniversityTemplate {...templateProps} />;
      case 'progress':
      default:
        return <ProgressTemplate {...templateProps} />;
    }
  };

  return <>{renderTemplate()}</>;
};

export default EducationSection; 