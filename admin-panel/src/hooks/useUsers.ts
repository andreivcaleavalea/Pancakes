import { useState, useEffect } from 'react';
import { adminApi, UserOverview, PagedResponse } from '../services/api';
import { message } from 'antd';

export const useUsers = () => {
  const [users, setUsers] = useState<PagedResponse<UserOverview> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [searchTerm, setSearchTerm] = useState('');

  const fetchUsers = async (page = currentPage, size = pageSize, search = searchTerm) => {
    try {
      setLoading(true);
      setError(null);
      const response = await adminApi.getUsers(page, size, search);
      
      if (response.success) {
        setUsers(response.data);
      } else {
        setError(response.message || 'Failed to fetch users');
        message.error('Failed to load users');
      }
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.message || 'Failed to fetch users';
      setError(errorMessage);
      message.error(errorMessage);
      console.error('Users fetch error:', err);
    } finally {
      setLoading(false);
    }
  };

  const banUser = async (userId: string, reason: string, expiresAt?: string) => {
    try {
      setActionLoading(userId);
      const response = await adminApi.banUser(userId, reason, expiresAt);
      
      if (response.success) {
        message.success('User banned successfully');
        await fetchUsers();
      } else {
        message.error(response.message || 'Failed to ban user');
      }
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.message || 'Failed to ban user';
      message.error(errorMessage);
      console.error('Ban user error:', err);
    } finally {
      setActionLoading(null);
    }
  };

  const unbanUser = async (userId: string, reason: string) => {
    try {
      setActionLoading(userId);
      const response = await adminApi.unbanUser(userId, reason);
      
      if (response.success) {
        message.success('User unbanned successfully');
        await fetchUsers();
      } else {
        message.error(response.message || 'Failed to unban user');
      }
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.message || 'Failed to unban user';
      message.error(errorMessage);
      console.error('Unban user error:', err);
    } finally {
      setActionLoading(null);
    }
  };

  const search = (term: string) => {
    setSearchTerm(term);
    setCurrentPage(1);
    fetchUsers(1, pageSize, term);
  };

  const changePage = (page: number, size?: number) => {
    setCurrentPage(page);
    if (size) setPageSize(size);
    fetchUsers(page, size || pageSize, searchTerm);
  };

  const refresh = () => {
    fetchUsers();
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  return {
    users,
    loading,
    error,
    actionLoading,
    currentPage,
    pageSize,
    searchTerm,
    banUser,
    unbanUser,
    search,
    changePage,
    refresh
  };
};