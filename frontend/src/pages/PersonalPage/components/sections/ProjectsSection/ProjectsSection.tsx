import React from 'react';
import type { Project, SectionSettings, TemplateOption } from '../../../types';
import { SECTION_COLORS } from '../../../constants';
import { 
  CardsTemplate, 
  DetailedTemplate, 
  GridTemplate, 
  MinimalTemplate, 
  PortfolioTemplate, 
  ShowcaseTemplate 
} from './templates';

interface ProjectsSectionProps {
  sectionKey: string;
  projects: Project[];
  primaryColor: string;
  currentSectionSettings: SectionSettings;
  onSectionSettingsChange: (sectionKey: string, newSettings: SectionSettings) => void;
  templateOptions: TemplateOption[];
  editMode?: boolean;
}

const ProjectsSection: React.FC<ProjectsSectionProps> = ({
  sectionKey,
  projects,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
  editMode = true,
}) => {
  const { template, advancedSettings } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

  // Common props for all templates
  const templateProps = {
    projects,
    sectionKey,
    sectionPrimaryColor,
    currentSectionSettings,
    onSectionSettingsChange,
    templateOptions,
    advancedSettings,
    editMode,
  };

  // Template selector
  const renderTemplate = () => {
    switch (template) {
      case 'cards':
        return <CardsTemplate {...templateProps} />;
      case 'detailed':
        return <DetailedTemplate {...templateProps} />;
      case 'grid':
        return <GridTemplate {...templateProps} />;
      case 'minimal':
        return <MinimalTemplate {...templateProps} />;
      case 'portfolio':
        return <PortfolioTemplate {...templateProps} />;
      case 'showcase':
      default:
        return <ShowcaseTemplate {...templateProps} />;
    }
  };

  return <>{renderTemplate()}</>;
};

export default ProjectsSection; 