import type { AllTemplateOptions } from './types';
import type { AdvancedSectionSettings } from '../../services/personalPageService';

export const SECTION_COLORS = {
  blue: '#1890ff',
  green: '#52c41a',
  purple: '#722ed1',
  cyan: '#13c2c2',
  magenta: '#eb2f96',
  volcano: '#fa541c',
  orange: '#fa8c16',
  red: '#f5222d',
  gold: '#faad14'
};

// Default advanced settings for all sections
const DEFAULT_ADVANCED_SETTINGS: AdvancedSectionSettings = {
  layout: {
    fullscreen: false,
    margin: 16
  },
  background: {
    color: "",
    pattern: "none",
    opacity: 1.0
  },
  typography: {
    fontSize: "medium",
    fontColor: "",
    fontWeight: "normal"
  },
  styling: {
    roundCorners: true,
    borderRadius: "8px",
    shadow: true,
    shadowIntensity: "medium",
    border: {
      enabled: false,
      color: "#e5e7eb",
      width: "1px",
      style: "solid"
    }
  }
};

export const ALL_TEMPLATE_OPTIONS: AllTemplateOptions = {
  personal: [
    { value: 'card', label: '👤 Simple Card' },
    { value: 'hero', label: '🎯 Hero Section' },
    { value: 'minimal', label: '✨ Minimal Profile' },
    { value: 'professional', label: '💼 Professional Showcase' },
    { value: 'creative', label: '🎨 Creative Profile' },
    { value: 'modern', label: '🚀 Modern Layout' },
  ],
  education: [
    { value: 'timeline', label: '📚 Timeline' },
    { value: 'academic', label: '🎓 Academic Timeline' },
    { value: 'grid', label: '📋 Grid Layout' },
    { value: 'journey', label: '🛤️ Education Journey' },
    { value: 'university', label: '🏛️ University Style' },
    { value: 'progress', label: '📊 Progress Timeline' },
  ],
  jobs: [
    { value: 'career', label: '🚀 Career Journey' },
    { value: 'corporate', label: '🏢 Corporate Timeline' },
    { value: 'timeline', label: '💼 Simple Timeline' },
    { value: 'professional', label: '👔 Professional Showcase' },
    { value: 'experience', label: '⭐ Experience Cards' },
    { value: 'roadmap', label: '🗺️ Career Roadmap' },
  ],
  projects: [
    { value: 'grid', label: '🎨 Creative Grid' },
    { value: 'showcase', label: '🚀 Project Showcase' },
    { value: 'minimal', label: '📝 Simple List' },
    { value: 'portfolio', label: '🖼️ Portfolio Gallery' },
    { value: 'cards', label: '🃏 Project Cards' },
    { value: 'detailed', label: '📋 Detailed View' },
  ],
  hobbies: [
    { value: 'tags', label: '🏷️ Tag Cloud' },
    { value: 'creative', label: '🎨 Creative Layout' },
    { value: 'minimal', label: '✨ Simple Tags' },
    { value: 'interests', label: '🎯 Interest Cards' },
    { value: 'colorful', label: '🌈 Colorful Display' },
    { value: 'icons', label: '🎪 Icon Gallery' },
  ],
};

export const DEFAULT_SECTION_TEMPLATES = {
  personal: 'card',
  education: 'timeline',
  jobs: 'timeline',
  projects: 'grid',
  hobbies: 'tags'
};

export const DEFAULT_SECTION_ORDER = ['personal', 'education', 'jobs', 'projects', 'hobbies'];

export const DEFAULT_SECTION_VISIBILITY = {
  personal: true,
  education: true,
  jobs: true,
  projects: true,
  hobbies: true
};

export const DEFAULT_SECTION_SETTINGS = {
  personal: { template: 'card', color: 'blue', advancedSettings: DEFAULT_ADVANCED_SETTINGS },
  education: { template: 'timeline', color: 'green', advancedSettings: DEFAULT_ADVANCED_SETTINGS },
  jobs: { template: 'career', color: 'blue', advancedSettings: DEFAULT_ADVANCED_SETTINGS },
  projects: { template: 'grid', color: 'purple', advancedSettings: DEFAULT_ADVANCED_SETTINGS },
  hobbies: { template: 'tags', color: 'orange', advancedSettings: DEFAULT_ADVANCED_SETTINGS },
};

export const DEFAULT_PUBLIC_SETTINGS = {
  isPublic: false,
  pageSlug: '',
}; 