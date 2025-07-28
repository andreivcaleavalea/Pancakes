import React from 'react';
import { PersonalSection, EducationSection, JobsSection, ProjectsSection, HobbiesSection } from './sections';
import { SECTION_COLORS, ALL_TEMPLATE_OPTIONS } from '../constants';
import type { 
  User, 
  Education, 
  Job, 
  Project, 
  Hobby, 
  SectionVisibility, 
  SectionSettingsMap,
  SectionSettings 
} from '../types';

interface PersonalPageSectionsProps {
  sectionOrder: string[];
  sectionVisibility: SectionVisibility;
  sectionSettings: SectionSettingsMap;
  user: User | null;
  educations: Education[];
  jobs: Job[];
  projects: Project[];
  hobbies: Hobby[];
  onSectionSettingsChange: (sectionKey: string, newSettings: SectionSettings) => void;
}

/**
 * Component responsible for rendering all personal page sections
 * Follows Single Responsibility Principle - only handles section rendering
 */
export const PersonalPageSections: React.FC<PersonalPageSectionsProps> = ({
  sectionOrder,
  sectionVisibility,
  sectionSettings,
  user,
  educations,
  jobs,
  projects,
  hobbies,
  onSectionSettingsChange,
}) => {
  
  // Helper function to get section data
  const getSectionData = (sectionKey: string) => {
    switch (sectionKey) {
      case 'personal':
        return user;
      case 'education':
        return educations;
      case 'jobs':
        return jobs;
      case 'projects':
        return projects;
      case 'hobbies':
        return hobbies;
      default:
        return null;
    }
  };

  // Helper function to check if section should render
  const shouldRenderSection = (sectionKey: string, data: any) => {
    // Check visibility setting
    if (!sectionVisibility[sectionKey as keyof SectionVisibility]) {
      return false;
    }

    // Check data availability
    switch (sectionKey) {
      case 'personal':
        return !!data;
      case 'education':
      case 'jobs':
      case 'projects':
      case 'hobbies':
        return Array.isArray(data) && data.length > 0;
      default:
        return false;
    }
  };

  const renderSection = (sectionKey: string) => {
    // Get section data
    const data = getSectionData(sectionKey);
    
    // Check if section should render
    if (!shouldRenderSection(sectionKey, data)) {
      return null;
    }

    // Get current section settings
    const currentSectionSettings = sectionSettings[sectionKey as keyof SectionSettingsMap];
    if (!currentSectionSettings) {
      return null;
    }

    // Get section primary color
    const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;
    
    // Get template options
    const templateOptions = ALL_TEMPLATE_OPTIONS[sectionKey as keyof typeof ALL_TEMPLATE_OPTIONS] || [];

    // Create section props
    const sectionProps = {
      sectionKey,
      user,
      primaryColor: '#1890ff', // Global primary color
      sectionPrimaryColor,
      currentSectionSettings,
      onSectionSettingsChange,
      templateOptions,
      advancedSettings: currentSectionSettings.advancedSettings,
      editMode: true,
    };

    // Render appropriate section component
    switch (sectionKey) {
      case 'personal':
        if (!user) return null;
        return (
          <PersonalSection
            key={sectionKey}
            {...sectionProps}
            user={user}
          />
        );
      
      case 'education':
        return (
          <EducationSection
            key={sectionKey}
            {...sectionProps}
            educations={data as Education[]}
          />
        );
      
      case 'jobs':
        return (
          <JobsSection
            key={sectionKey}
            {...sectionProps}
            jobs={data as Job[]}
          />
        );
      
      case 'projects':
        return (
          <ProjectsSection
            key={sectionKey}
            {...sectionProps}
            projects={data as Project[]}
          />
        );
      
      case 'hobbies':
        return (
          <HobbiesSection
            key={sectionKey}
            {...sectionProps}
            hobbies={data as Hobby[]}
          />
        );
      
      default:
        return null;
    }
  };

  return (
    <>
      {sectionOrder.map((sectionKey: string) => renderSection(sectionKey))}
    </>
  );
};
