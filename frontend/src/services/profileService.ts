import type { UserProfile, Education, Job, Hobby, Project, ProfileData } from '../types/profile';
import { authenticatedFetch } from '../lib/api';

export class ProfileService {
  private static readonly BASE_URL = import.meta.env.VITE_USER_API_URL || 'http://localhost:5141';

  // Get complete profile data
  static async getProfileData(): Promise<ProfileData> {
    const response = await authenticatedFetch<ProfileData>('api/profile');
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  // Get user profile
  static async getUserProfile(): Promise<UserProfile> {
    const profileData = await this.getProfileData();
    return profileData.user;
  }

  // Update user profile
  static async updateUserProfile(profile: Partial<UserProfile>): Promise<UserProfile> {
    const response = await authenticatedFetch<UserProfile>('api/profile/user', {
      method: 'PUT',
      body: JSON.stringify(profile)
    });
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  // Education APIs
  static async getEducations(): Promise<Education[]> {
    const response = await authenticatedFetch<Education[]>('api/profile/educations');
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  static async addEducation(education: Omit<Education, 'id'>): Promise<Education> {
    const response = await authenticatedFetch<Education>('api/profile/educations', {
      method: 'POST',
      body: JSON.stringify(education)
    });
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  static async updateEducation(id: string, education: Partial<Education>): Promise<Education> {
    const response = await authenticatedFetch<Education>(`api/profile/educations/${id}`, {
      method: 'PUT',
      body: JSON.stringify(education)
    });
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  static async deleteEducation(id: string): Promise<void> {
    const response = await authenticatedFetch(`api/profile/educations/${id}`, {
      method: 'DELETE'
    });
    
    if (response.error) {
      throw new Error(response.error);
    }
  }

  // Job APIs
  static async getJobs(): Promise<Job[]> {
    const response = await authenticatedFetch<Job[]>('api/profile/jobs');
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  static async addJob(job: Omit<Job, 'id'>): Promise<Job> {
    const response = await authenticatedFetch<Job>('api/profile/jobs', {
      method: 'POST',
      body: JSON.stringify(job)
    });
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  static async updateJob(id: string, job: Partial<Job>): Promise<Job> {
    const response = await authenticatedFetch<Job>(`api/profile/jobs/${id}`, {
      method: 'PUT',
      body: JSON.stringify(job)
    });
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  static async deleteJob(id: string): Promise<void> {
    const response = await authenticatedFetch(`api/profile/jobs/${id}`, {
      method: 'DELETE'
    });
    
    if (response.error) {
      throw new Error(response.error);
    }
  }

  // Hobby APIs
  static async getHobbies(): Promise<Hobby[]> {
    const response = await authenticatedFetch<Hobby[]>('api/profile/hobbies');
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  static async addHobby(hobby: Omit<Hobby, 'id'>): Promise<Hobby> {
    const response = await authenticatedFetch<Hobby>('api/profile/hobbies', {
      method: 'POST',
      body: JSON.stringify(hobby)
    });
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  static async updateHobby(id: string, hobby: Partial<Hobby>): Promise<Hobby> {
    const response = await authenticatedFetch<Hobby>(`api/profile/hobbies/${id}`, {
      method: 'PUT',
      body: JSON.stringify(hobby)
    });
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  static async deleteHobby(id: string): Promise<void> {
    const response = await authenticatedFetch(`api/profile/hobbies/${id}`, {
      method: 'DELETE'
    });
    
    if (response.error) {
      throw new Error(response.error);
    }
  }

  // Project APIs
  static async getProjects(): Promise<Project[]> {
    const response = await authenticatedFetch<Project[]>('api/profile/projects');
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  static async addProject(project: Omit<Project, 'id'>): Promise<Project> {
    const response = await authenticatedFetch<Project>('api/profile/projects', {
      method: 'POST',
      body: JSON.stringify(project)
    });
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  static async updateProject(id: string, project: Partial<Project>): Promise<Project> {
    const response = await authenticatedFetch<Project>(`api/profile/projects/${id}`, {
      method: 'PUT',
      body: JSON.stringify(project)
    });
    
    if (response.error) {
      throw new Error(response.error);
    }

    return response.data!;
  }

  static async deleteProject(id: string): Promise<void> {
    const response = await authenticatedFetch(`api/profile/projects/${id}`, {
      method: 'DELETE'
    });
    
    if (response.error) {
      throw new Error(response.error);
    }
  }
}
