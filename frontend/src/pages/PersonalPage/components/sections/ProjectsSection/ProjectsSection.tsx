import React from 'react';
import type { SectionRendererProps } from '../../../types';
import { SECTION_COLORS } from '../../../constants';
import { 
  GridTemplate, 
  ShowcaseTemplate, 
  MinimalTemplate, 
  PortfolioTemplate, 
  CardsTemplate, 
  DetailedTemplate 
} from './templates';

interface ProjectsSectionProps extends Omit<SectionRendererProps, 'data'> {
  projects: any[];
}

const ProjectsSection: React.FC<ProjectsSectionProps> = ({
  sectionKey,
  projects,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  const { template } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

  // Common props for all templates
  const templateProps = {
    projects,
    sectionKey,
    sectionPrimaryColor,
    currentSectionSettings,
    onSectionSettingsChange,
    templateOptions,
  };

  // Template selector
  switch (template) {
    case 'grid':
      return <GridTemplate {...templateProps} />;
    case 'showcase':
      return <ShowcaseTemplate {...templateProps} />;
    case 'minimal':
      return <MinimalTemplate {...templateProps} />;
    case 'portfolio':
      return <PortfolioTemplate {...templateProps} />;
    case 'cards':
      return <CardsTemplate {...templateProps} />;
    case 'detailed':
      return <DetailedTemplate {...templateProps} />;
    default:
      return <GridTemplate {...templateProps} />; // fallback
  }
};

export default ProjectsSection; 