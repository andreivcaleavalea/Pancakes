import { authenticatedFetch } from '../lib/api';

export interface PersonalPageSettings {
  id: string;
  userId: string;
  isPublic: boolean;
  pageSlug?: string;
  sectionOrder: string[];
  sectionVisibility: Record<string, boolean>;
  sectionTemplates: Record<string, string>;
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
  private static readonly BASE_URL = import.meta.env.VITE_USER_API_URL || 'http://localhost:5141';

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
    const response = await authenticatedFetch<PublicPersonalPage>(`api/personalpage/public/${pageSlug}`);
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
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
      throw new Error(response.error);
    }

    return response.data!.slug;
  }
} 