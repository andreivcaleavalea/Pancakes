export interface SectionSettings {
  template: string;
  color: string;
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

export interface SectionRendererProps {
  sectionKey: string;
  user: any;
  data: any[];
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
} 