import { useState, useEffect } from 'react';
import type { UserProfile, Education, Job, Hobby, Project, ProfileData } from '../types/profile';
import { ProfileService } from '../services/profileService';

export const useProfile = () => {
  const [profileData, setProfileData] = useState<ProfileData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchProfileData = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await ProfileService.getProfileData();
      setProfileData(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load profile data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProfileData();
  }, []);

  const updateUserProfile = async (profile: Partial<UserProfile>) => {
    try {
      const updatedProfile = await ProfileService.updateUserProfile(profile);
      setProfileData(prev => prev ? { ...prev, user: updatedProfile } : null);
      return updatedProfile;
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to update profile');
    }
  };

  const uploadProfilePicture = async (file: File) => {
    try {
      const updatedProfile = await ProfileService.uploadProfilePicture(file);
      setProfileData(prev => prev ? { ...prev, user: updatedProfile } : null);
      return updatedProfile;
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to upload profile picture');
    }
  };

  return {
    profileData,
    loading,
    error,
    refetch: fetchProfileData,
    updateUserProfile,
    uploadProfilePicture
  };
};

export const useEducations = () => {
  const [educations, setEducations] = useState<Education[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchEducations = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await ProfileService.getEducations();
      setEducations(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load educations');
    } finally {
      setLoading(false);
    }
  };

  const addEducation = async (education: Omit<Education, 'id'>) => {
    try {
      const newEducation = await ProfileService.addEducation(education);
      setEducations(prev => [...prev, newEducation]);
      return newEducation;
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to add education');
    }
  };

  const updateEducation = async (id: string, education: Partial<Education>) => {
    try {
      const updatedEducation = await ProfileService.updateEducation(id, education);
      setEducations(prev => prev.map(e => e.id === id ? updatedEducation : e));
      return updatedEducation;
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to update education');
    }
  };

  const deleteEducation = async (id: string) => {
    try {
      await ProfileService.deleteEducation(id);
      setEducations(prev => prev.filter(e => e.id !== id));
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to delete education');
    }
  };

  useEffect(() => {
    fetchEducations();
  }, []);

  return {
    educations,
    loading,
    error,
    addEducation,
    updateEducation,
    deleteEducation,
    refetch: fetchEducations
  };
};

export const useJobs = () => {
  const [jobs, setJobs] = useState<Job[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchJobs = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await ProfileService.getJobs();
      setJobs(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load jobs');
    } finally {
      setLoading(false);
    }
  };

  const addJob = async (job: Omit<Job, 'id'>) => {
    try {
      const newJob = await ProfileService.addJob(job);
      setJobs(prev => [...prev, newJob]);
      return newJob;
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to add job');
    }
  };

  const updateJob = async (id: string, job: Partial<Job>) => {
    try {
      const updatedJob = await ProfileService.updateJob(id, job);
      setJobs(prev => prev.map(j => j.id === id ? updatedJob : j));
      return updatedJob;
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to update job');
    }
  };

  const deleteJob = async (id: string) => {
    try {
      await ProfileService.deleteJob(id);
      setJobs(prev => prev.filter(j => j.id !== id));
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to delete job');
    }
  };

  useEffect(() => {
    fetchJobs();
  }, []);

  return {
    jobs,
    loading,
    error,
    addJob,
    updateJob,
    deleteJob,
    refetch: fetchJobs
  };
};

export const useHobbies = () => {
  const [hobbies, setHobbies] = useState<Hobby[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchHobbies = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await ProfileService.getHobbies();
      setHobbies(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load hobbies');
    } finally {
      setLoading(false);
    }
  };

  const addHobby = async (hobby: Omit<Hobby, 'id'>) => {
    try {
      const newHobby = await ProfileService.addHobby(hobby);
      setHobbies(prev => [...prev, newHobby]);
      return newHobby;
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to add hobby');
    }
  };

  const updateHobby = async (id: string, hobby: Partial<Hobby>) => {
    try {
      const updatedHobby = await ProfileService.updateHobby(id, hobby);
      setHobbies(prev => prev.map(h => h.id === id ? updatedHobby : h));
      return updatedHobby;
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to update hobby');
    }
  };

  const deleteHobby = async (id: string) => {
    try {
      await ProfileService.deleteHobby(id);
      setHobbies(prev => prev.filter(h => h.id !== id));
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to delete hobby');
    }
  };

  useEffect(() => {
    fetchHobbies();
  }, []);

  return {
    hobbies,
    loading,
    error,
    addHobby,
    updateHobby,
    deleteHobby,
    refetch: fetchHobbies
  };
};

export const useProjects = () => {
  const [projects, setProjects] = useState<Project[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchProjects = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await ProfileService.getProjects();
      setProjects(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load projects');
    } finally {
      setLoading(false);
    }
  };

  const addProject = async (project: Omit<Project, 'id'>) => {
    try {
      const newProject = await ProfileService.addProject(project);
      setProjects(prev => [...prev, newProject]);
      return newProject;
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to add project');
    }
  };

  const updateProject = async (id: string, project: Partial<Project>) => {
    try {
      const updatedProject = await ProfileService.updateProject(id, project);
      setProjects(prev => prev.map(p => p.id === id ? updatedProject : p));
      return updatedProject;
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to update project');
    }
  };

  const deleteProject = async (id: string) => {
    try {
      await ProfileService.deleteProject(id);
      setProjects(prev => prev.filter(p => p.id !== id));
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to delete project');
    }
  };

  useEffect(() => {
    fetchProjects();
  }, []);

  return {
    projects,
    loading,
    error,
    addProject,
    updateProject,
    deleteProject,
    refetch: fetchProjects
  };
};
