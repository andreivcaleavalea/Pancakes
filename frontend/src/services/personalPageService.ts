import { authenticatedFetch } from '../lib/api';

export interface AdvancedSectionSettings {
  layout: LayoutSettings;
  background: BackgroundSettings;
  typography: TypographySettings;
  styling: StylingSettings;
}

export interface LayoutSettings {
  fullscreen: boolean;
  margin: string | number; // "8px", "16px", "24px", "32px" or number for slider
}

export interface BackgroundSettings {
  color: string;
  pattern: string; // "none", "dots", "grid", "diagonal", "waves"
  opacity: number; // 0.0 to 1.0
}

export interface TypographySettings {
  fontSize: string; // "small", "medium", "large", "xl"
  fontColor: string;
  fontWeight: string; // "normal", "medium", "semibold", "bold"
}

export interface StylingSettings {
  roundCorners: boolean;
  borderRadius: string; // "4px", "8px", "12px", "16px", "24px"
  shadow: boolean;
  shadowIntensity: string; // "light", "medium", "strong"
  border: BorderSettings;
}

export interface BorderSettings {
  enabled: boolean;
  color: string;
  width: string; // "1px", "2px", "3px"
  style: string; // "solid", "dashed", "dotted"
}



export interface PersonalPageSettings {
  id: string;
  userId: string;
  isPublic: boolean;
  pageSlug?: string;
  sectionOrder: string[];
  sectionVisibility: Record<string, boolean>;
  sectionTemplates: Record<string, string>;
  sectionColors: Record<string, string>;
  sectionAdvancedSettings: Record<string, AdvancedSectionSettings>;
  theme: string;
  colorScheme: string;
  createdAt: string;
  updatedAt: string;
}

export interface UpdatePersonalPageSettings {
  isPublic?: boolean;
  pageSlug?: string;
  sectionOrder?: string[];
  sectionVisibility?: Record<string, boolean>;
  sectionTemplates?: Record<string, string>;
  sectionColors?: Record<string, string>;
  sectionAdvancedSettings?: Record<string, AdvancedSectionSettings>;
  theme?: string;
  colorScheme?: string;
}

export interface PublicPersonalPage {
  user: any; 
  educations: any[];
  jobs: any[];
  hobbies: any[];
  projects: any[];
  settings: PersonalPageSettings;
}

export class PersonalPageService {


  // Get personal page settings
  static async getSettings(): Promise<PersonalPageSettings> {
    const response = await authenticatedFetch<PersonalPageSettings>('api/personalpage/settings');
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  // Update personal page settings
  static async updateSettings(settings: UpdatePersonalPageSettings): Promise<PersonalPageSettings> {
    const response = await authenticatedFetch<PersonalPageSettings>('api/personalpage/settings', {
      method: 'PUT',
      body: JSON.stringify(settings)
    });
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  // Get public personal page
  static async getPublicPage(pageSlug: string): Promise<PublicPersonalPage> {
    try {
      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/PersonalPage/public/${pageSlug}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        const errorText = await response.text();
        console.error('PersonalPageService: Error response:', response.status, errorText);
        throw new Error(`Failed to fetch public page: ${response.status}`);
      }

      const data = await response.json();
      return data;
    } catch (error) {
      throw error;
    }
  }

  // Get preview of personal page
  static async getPreview(): Promise<PublicPersonalPage> {
    const response = await authenticatedFetch<PublicPersonalPage>('api/personalpage/preview');
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  // Generate unique slug
  static async generateSlug(baseName: string): Promise<string> {
    const response = await authenticatedFetch<{ slug: string }>('api/personalpage/generate-slug', {
      method: 'POST',
      body: JSON.stringify({ baseName })
    });
    
    if (response.error) {
      console.error('PersonalPageService: Generate slug error:', response.error);
      throw new Error(response.error);
    }

    const slug = response.data!.slug;
    return slug;
  }
} 