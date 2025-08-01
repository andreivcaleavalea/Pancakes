import { useState, useEffect } from 'react';
import { adminApi, ContentFlag, PagedResponse } from '../services/api';
import { message } from 'antd';

export const useContentModeration = () => {
  const [flags, setFlags] = useState<PagedResponse<ContentFlag> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [statusFilter, setStatusFilter] = useState('');
  const [contentTypeFilter, setContentTypeFilter] = useState('');

  const fetchFlags = async (
    page = currentPage, 
    size = pageSize, 
    status = statusFilter, 
    contentType = contentTypeFilter
  ) => {
    try {
      setLoading(true);
      setError(null);
      const response = await adminApi.getContentFlags(page, size, status, contentType);
      
      if (response.success) {
        setFlags(response.data);
      } else {
        setError(response.message || 'Failed to fetch content flags');
        message.error('Failed to load content flags');
      }
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.message || 'Failed to fetch content flags';
      setError(errorMessage);
      message.error(errorMessage);
      console.error('Content flags fetch error:', err);
    } finally {
      setLoading(false);
    }
  };

  const reviewFlag = async (flagId: string, action: 'approve' | 'reject', reason: string) => {
    try {
      setActionLoading(flagId);
      const response = await adminApi.reviewFlag(flagId, action, reason);
      
      if (response.success) {
        message.success(`Content ${action}d successfully`);
        await fetchFlags();
      } else {
        message.error(response.message || `Failed to ${action} content`);
      }
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.message || `Failed to ${action} content`;
      message.error(errorMessage);
      console.error(`${action} content error:`, err);
    } finally {
      setActionLoading(null);
    }
  };

  const approveContent = (flagId: string, reason: string) => reviewFlag(flagId, 'approve', reason);
  const rejectContent = (flagId: string, reason: string) => reviewFlag(flagId, 'reject', reason);

  const filterByStatus = (status: string) => {
    setStatusFilter(status);
    setCurrentPage(1);
    fetchFlags(1, pageSize, status, contentTypeFilter);
  };

  const filterByContentType = (contentType: string) => {
    setContentTypeFilter(contentType);
    setCurrentPage(1);
    fetchFlags(1, pageSize, statusFilter, contentType);
  };

  const changePage = (page: number, size?: number) => {
    setCurrentPage(page);
    if (size) setPageSize(size);
    fetchFlags(page, size || pageSize, statusFilter, contentTypeFilter);
  };

  const refresh = () => {
    fetchFlags();
  };

  useEffect(() => {
    fetchFlags();
  }, []);

  return {
    flags,
    loading,
    error,
    actionLoading,
    currentPage,
    pageSize,
    statusFilter,
    contentTypeFilter,
    approveContent,
    rejectContent,
    filterByStatus,
    filterByContentType,
    changePage,
    refresh
  };
};