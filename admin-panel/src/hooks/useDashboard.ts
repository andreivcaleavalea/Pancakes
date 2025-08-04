import { useState, useEffect } from 'react';
import { adminApi, DashboardStats } from '../services/api';
import { message } from 'antd';

export const useDashboard = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchDashboardStats = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await adminApi.getDashboardStats();
      
      if (response.success) {
        setStats(response.data);
      } else {
        setError(response.message || 'Failed to fetch dashboard stats');
        message.error('Failed to load dashboard data');
      }
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.message || 'Failed to fetch dashboard stats';
      setError(errorMessage);
      message.error(errorMessage);
      console.error('Dashboard fetch error:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboardStats();
  }, []);

  const refresh = () => {
    fetchDashboardStats();
  };

  return {
    stats,
    loading,
    error,
    refresh
  };
};