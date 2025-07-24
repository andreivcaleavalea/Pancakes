import React from 'react';
import type { Job, SectionSettings, TemplateOption } from '../../../types';
import { SECTION_COLORS } from '../../../constants';
import { 
  CareerTemplate, 
  CorporateTemplate, 
  TimelineTemplate, 
  ProfessionalTemplate, 
  ExperienceTemplate, 
  RoadmapTemplate 
} from './templates';

interface JobsSectionProps {
  sectionKey: string;
  jobs: Job[];
  primaryColor: string;
  currentSectionSettings: SectionSettings;
  onSectionSettingsChange: (sectionKey: string, newSettings: SectionSettings) => void;
  templateOptions: TemplateOption[];
}

const JobsSection: React.FC<JobsSectionProps> = ({
  sectionKey,
  jobs,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  const { template, advancedSettings } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

  // Common props for all templates
  const templateProps = {
    jobs,
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
      default:
        return <RoadmapTemplate {...templateProps} />;
    }
  };

  return <>{renderTemplate()}</>;
};

export default JobsSection; 