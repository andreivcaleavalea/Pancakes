import { useProfile } from '../../../hooks/useProfile';
import { usePersonalPage } from '../../../hooks/usePersonalPage';
import type { User, Education, Job, Project, Hobby } from '../types';

/**
 * Hook responsible for fetching and providing personal page data
 */
export const usePersonalPageData = () => {
  const { 
    profileData, 
    loading: profileLoading, 
    error: profileError 
  } = useProfile();
  
  const { 
    settings: originalSettings, 
    loading: settingsLoading, 
    error: settingsError, 
    refetch 
  } = usePersonalPage();

  // Extract and structure data
  const user: User | null = profileData?.user || null;
  
  // Map global types to PersonalPage types
  const educations: Education[] = (profileData?.educations || []).map(edu => ({
    ...edu,
    field: edu.specialization // Map specialization to field
  }));
  
  const jobs: Job[] = profileData?.jobs || [];
  
  const projects: Project[] = (profileData?.projects || []).map(project => ({
    ...project,
    technologies: project.technologies ? project.technologies.split(', ') : [], // Convert string to array
    status: 'completed' as const, // Default status
    githubUrl: project.githubUrl,
    liveUrl: project.projectUrl
  }));
  
  const hobbies: Hobby[] = (profileData?.hobbies || []).map(hobby => ({
    ...hobby,
    level: hobby.level?.toLowerCase() as 'beginner' | 'intermediate' | 'advanced' | 'expert' | undefined
  }));

  // Combine loading and error states
  const isLoading = profileLoading || settingsLoading;
  const error = profileError || settingsError;

  return {
    // Core data
    user,
    educations,
    jobs,
    projects,
    hobbies,
    
    // Original settings for comparison
    originalSettings,
    
    // State indicators
    isLoading,
    error,
    
    // Actions
    refetchSettings: refetch,
  };
};
