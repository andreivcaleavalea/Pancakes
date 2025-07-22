import React from 'react';
import type { SectionRendererProps } from '../../../types';
import { SECTION_COLORS } from '../../../constants';
import { 
  CareerTemplate, 
  CorporateTemplate, 
  TimelineTemplate, 
  ProfessionalTemplate, 
  ExperienceTemplate, 
  RoadmapTemplate 
} from './templates';

interface JobsSectionProps extends Omit<SectionRendererProps, 'data'> {
  jobs: any[];
}

const JobsSection: React.FC<JobsSectionProps> = ({
  sectionKey,
  jobs,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  const { template } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

  // Common props for all templates
  const templateProps = {
    jobs,
    sectionKey,
    sectionPrimaryColor,
    currentSectionSettings,
    onSectionSettingsChange,
    templateOptions,
  };

  // Template selector
  switch (template) {
    case 'career':
      return <CareerTemplate {...templateProps} />;
    case 'corporate':
      return <CorporateTemplate {...templateProps} />;
    case 'timeline':
      return <TimelineTemplate {...templateProps} />;
    case 'professional':
      return <ProfessionalTemplate {...templateProps} />;
    case 'experience':
      return <ExperienceTemplate {...templateProps} />;
    case 'roadmap':
      return <RoadmapTemplate {...templateProps} />;
    default:
      return <TimelineTemplate {...templateProps} />; // fallback
  }
};

export default JobsSection; 