import { useState, useEffect, useCallback } from 'react';
import { PersonalPageService } from '@/services/personalPageService';
import type { PublicPersonalPage } from '@/services/personalPageService';

interface PaginationInfo {
  currentPage: number;
  totalPages: number;
  totalItems: number;
  pageSize: number;
}

export const usePaginatedPortfolios = (page: number = 1, pageSize: number = 9) => {
  const [data, setData] = useState<PublicPersonalPage[]>([]);
  const [pagination, setPagination] = useState<PaginationInfo>({
    currentPage: 1,
    totalPages: 1,
    totalItems: 0,
    pageSize: pageSize
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchPortfolios = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Fetch paginated portfolios from backend
      const result = await PersonalPageService.getPublicPortfolios(page, pageSize);
      
      setData(result.data);
      setPagination({
        currentPage: result.pagination.currentPage || page,
        totalPages: result.pagination.totalPages || 1,
        totalItems: result.pagination.totalItems || 0,
        pageSize: result.pagination.pageSize || pageSize
      });
    } catch (err) {
      console.error('Error fetching paginated portfolios:', err);
      setError(err instanceof Error ? err.message : 'Failed to load portfolios');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => {
    fetchPortfolios();
  }, [fetchPortfolios]);

  return { data, pagination, loading, error, refetch: fetchPortfolios };
};
