import React from 'react';
import type { SectionRendererProps } from '../../../types';
import { SECTION_COLORS } from '../../../constants';
import { 
  AcademicTemplate, 
  TimelineTemplate, 
  GridTemplate, 
  JourneyTemplate, 
  UniversityTemplate, 
  ProgressTemplate 
} from './templates';

interface EducationSectionProps extends Omit<SectionRendererProps, 'data'> {
  educations: any[];
}

const EducationSection: React.FC<EducationSectionProps> = ({
  sectionKey,
  educations,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  const { template } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

  // Common props for all templates
  const templateProps = {
    educationData: educations,
    sectionKey,
    sectionPrimaryColor,
    currentSectionSettings,
    onSectionSettingsChange,
    templateOptions,
  };

  // Template selector
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
      return <ProgressTemplate {...templateProps} />;
    default:
      return <TimelineTemplate {...templateProps} />; // fallback
  }
};

export default EducationSection; 