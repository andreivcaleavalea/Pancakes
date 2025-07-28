import type { AdvancedSectionSettings } from '../../services/personalPageService';

// Core data interfaces
export interface User {
  id: string;
  name: string;
  email: string;
  avatar?: string;
  bio?: string;
  phoneNumber?: string;
  location?: string;
  website?: string;
  github?: string;
  linkedin?: string;
  twitter?: string;
}

export interface Education {
  id: string;
  institution: string;
  degree: string;
  field: string;
  startDate: string;
  endDate?: string;
  description?: string;
  gpa?: string;
  achievements?: string[];
}

export interface Job {
  id: string;
  company: string;
  position: string;
  startDate: string;
  endDate?: string;
  description?: string;
  technologies?: string[];
  achievements?: string[];
  location?: string;
  isCurrentRole?: boolean;
}

export interface Project {
  id: string;
  name: string;
  description: string;
  technologies: string[];
  startDate?: string;
  endDate?: string;
  githubUrl?: string;
  liveUrl?: string;
  imageUrl?: string;
  status: 'completed' | 'in-progress' | 'planned';
}

export interface Hobby {
  id: string;
  name: string;
  description?: string;
  category?: string;
  level?: 'beginner' | 'intermediate' | 'advanced' | 'expert';
  yearsOfExperience?: number;
}

// Settings interfaces
export interface SectionSettings {
  template: string;
  color: string;
  advancedSettings?: AdvancedSectionSettings;
}

export interface SectionSettingsMap {
  personal: SectionSettings;
  education: SectionSettings;
  jobs: SectionSettings;
  projects: SectionSettings;
  hobbies: SectionSettings;
}

export interface TemplateOption {
  value: string;
  label: string;
}

export interface AllTemplateOptions {
  personal: TemplateOption[];
  education: TemplateOption[];
  jobs: TemplateOption[];
  projects: TemplateOption[];
  hobbies: TemplateOption[];
}

export interface SectionVisibility {
  personal: boolean;
  education: boolean;
  jobs: boolean;
  projects: boolean;
  hobbies: boolean;
}

// Base section props (shared between all sections)
export interface BaseSectionProps {
  sectionKey: string;
  primaryColor: string;
  currentSectionSettings: SectionSettings;
  onSectionSettingsChange: (sectionKey: string, newSettings: SectionSettings) => void;
  templateOptions: TemplateOption[];
}

export interface SectionSettingsPopoverProps {
  sectionKey: string;
  sectionSettings: SectionSettings;
  onSettingsChange: (sectionKey: string, newSettings: SectionSettings) => void;
  templateOptions: TemplateOption[];
  editMode?: boolean;
}

// Template prop interfaces
export interface BaseTemplateProps {
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: SectionSettings;
  onSectionSettingsChange: (sectionKey: string, newSettings: SectionSettings) => void;
  templateOptions: TemplateOption[];
  advancedSettings?: AdvancedSectionSettings;
  editMode?: boolean; // Disable settings in public view
}

export interface PersonalTemplateProps extends BaseTemplateProps {
  user: User;
}

export interface EducationTemplateProps extends BaseTemplateProps {
  education: Education[];
}

export interface JobTemplateProps extends BaseTemplateProps {
  jobs: Job[];
}

export interface ProjectTemplateProps extends BaseTemplateProps {
  projects: Project[];
}

export interface HobbyTemplateProps extends BaseTemplateProps {
  hobbies: Hobby[];
} 