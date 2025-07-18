export interface UserProfile {
  id: string;
  name: string;
  email: string;
  avatar?: string;
  bio?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
}

export interface Education {
  id: string;
  institution: string;
  specialization: string;
  startDate: string;
  endDate?: string;
  degree: string;
  description?: string;
}

export interface Job {
  id: string;
  company: string;
  position: string;
  startDate: string;
  endDate?: string;
  description?: string;
  location?: string;
}

export interface Hobby {
  id: string;
  name: string;
  description?: string;
  level?: 'Beginner' | 'Intermediate' | 'Advanced' | 'Expert';
}

export interface Project {
  id: string;
  name: string;
  description: string;
  technologies: string; // Comma-separated string
  startDate: string;
  endDate?: string;
  projectUrl?: string;
  githubUrl?: string;
}

export interface ProfileData {
  user: UserProfile;
  educations: Education[];
  jobs: Job[];
  hobbies: Hobby[];
  projects: Project[];
}
