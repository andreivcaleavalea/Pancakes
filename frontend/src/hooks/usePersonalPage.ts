import { useState, useEffect } from 'react';
import { PersonalPageService } from '../services/personalPageService';
import type { PersonalPageSettings, UpdatePersonalPageSettings, PublicPersonalPage } from '../services/personalPageService';

export const usePersonalPage = () => {
  const [settings, setSettings] = useState<PersonalPageSettings | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchSettings = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await PersonalPageService.getSettings();
      setSettings(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load personal page settings');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSettings();
  }, []);

  const updateSettings = async (updates: UpdatePersonalPageSettings) => {
    try {
      const updatedSettings = await PersonalPageService.updateSettings(updates);
      setSettings(updatedSettings);
      return updatedSettings;
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to update settings');
    }
  };

  return {
    settings,
    loading,
    error,
    refetch: fetchSettings,
    updateSettings
  };
};

export const usePublicPersonalPage = (pageSlug: string) => {
  const [pageData, setPageData] = useState<PublicPersonalPage | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchPageData = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await PersonalPageService.getPublicPage(pageSlug);
      setPageData(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load personal page');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (pageSlug) {
      fetchPageData();
    }
  }, [pageSlug]);

  return {
    pageData,
    loading,
    error,
    refetch: fetchPageData
  };
};

export const usePersonalPagePreview = () => {
  const [pageData, setPageData] = useState<PublicPersonalPage | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchPreview = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await PersonalPageService.getPreview();
      setPageData(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load preview');
    } finally {
      setLoading(false);
    }
  };

  return {
    pageData,
    loading,
    error,
    fetchPreview
  };
}; 