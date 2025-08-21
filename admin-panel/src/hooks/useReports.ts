import { useState, useEffect, useCallback, useRef } from "react";
import { message } from "antd";
import { reportApi } from "@/services/reportApi";
import { ReportDto, ReportStatus, ReportStats } from "@/types/report";
import { useDebounce } from "./useDebounce";

// Constants for timing configurations
const DEFAULT_DEBOUNCE_MS = 300; // Default debounce delay for filters
const CACHE_DURATION = 60000; // 1 minute cache

interface UseReportsOptions {
  initialPageSize?: number;
  debounceMs?: number;
  enableCaching?: boolean;
}

interface ReportsState {
  reports: ReportDto[];
  stats: ReportStats;
  loading: boolean;
  actionLoading: boolean;
  error: string | null;
  currentPage: number;
  pageSize: number;
  total: number;
  selectedStatus?: ReportStatus;
}

// Simple in-memory cache for reports
const reportsCache = new Map<
  string,
  { data: ReportDto[]; timestamp: number }
>();
const statsCache = { data: null as ReportStats | null, timestamp: 0 };

export const useReports = (options: UseReportsOptions = {}) => {
  const {
    initialPageSize = 20, // Reduced from 100 to 20
    debounceMs = DEFAULT_DEBOUNCE_MS,
    enableCaching = true,
  } = options;

  const [state, setState] = useState<ReportsState>({
    reports: [],
    stats: { totalReports: 0, pendingReports: 0 },
    loading: false,
    actionLoading: false,
    error: null,
    currentPage: 1,
    pageSize: initialPageSize,
    total: 0,
    selectedStatus: undefined,
  });

  // Debounced filter status state
  const [filterStatus, setFilterStatus] = useState<ReportStatus | undefined>(
    undefined
  );
  const debouncedFilterStatus = useDebounce(filterStatus, debounceMs);

  // Cache busting utilities are not currently needed here

  // Request deduplication
  const requestInFlightRef = useRef<boolean>(false);

  // Cache utilities
  const getCacheKey = useCallback(
    (page: number, pageSize: number, status?: ReportStatus) =>
      `reports-${page}-${pageSize}-${status ?? "all"}`,
    []
  );

  const getCachedReports = useCallback(
    (cacheKey: string) => {
      if (!enableCaching) return null;
      const cached = reportsCache.get(cacheKey);
      if (cached && Date.now() - cached.timestamp < CACHE_DURATION) {
        return cached.data;
      }
      return null;
    },
    [enableCaching]
  );

  const setCachedReports = useCallback(
    (cacheKey: string, data: ReportDto[]) => {
      if (!enableCaching) return;
      reportsCache.set(cacheKey, { data, timestamp: Date.now() });
    },
    [enableCaching]
  );

  const getCachedStats = useCallback(() => {
    if (!enableCaching) return null;
    if (statsCache.data && Date.now() - statsCache.timestamp < CACHE_DURATION) {
      return statsCache.data;
    }
    return null;
  }, [enableCaching]);

  const setCachedStats = useCallback(
    (data: ReportStats) => {
      if (!enableCaching) return;
      statsCache.data = data;
      statsCache.timestamp = Date.now();
    },
    [enableCaching]
  );

  // Optimized fetch function with caching and deduplication
  const fetchReports = useCallback(
    async (
      page: number = state.currentPage,
      pageSize: number = state.pageSize,
      status?: ReportStatus,
      force: boolean = false
    ) => {
      // Prevent duplicate requests
      if (requestInFlightRef.current && !force) {
        return;
      }

      const cacheKey = getCacheKey(page, pageSize, status);

      // Try cache first
      if (!force) {
        const cachedReports = getCachedReports(cacheKey);
        const cachedStats = getCachedStats();

        if (cachedReports && cachedStats) {
          setState((prev) => ({
            ...prev,
            reports: cachedReports,
            stats: cachedStats,
            loading: false,
            error: null,
          }));
          return;
        }
      }

      try {
        setState((prev) => ({ ...prev, loading: true, error: null }));
        requestInFlightRef.current = true;

        // Fetch data concurrently but only if not cached
        const promises: Promise<any>[] = [];

        if (!force && !getCachedReports(cacheKey)) {
          promises.push(reportApi.getAll(page, pageSize, status));
        }

        if (!force && !getCachedStats()) {
          promises.push(reportApi.getStats());
        }

        // If everything is cached, just use cache
        if (promises.length === 0) {
          const cachedReports = getCachedReports(cacheKey);
          const cachedStats = getCachedStats();
          setState((prev) => ({
            ...prev,
            reports: cachedReports || [],
            stats: cachedStats || { totalReports: 0, pendingReports: 0 },
            loading: false,
          }));
          return;
        }

        const results = await Promise.all(promises);

        let reportsData: ReportDto[] = [];
        let statsData: ReportStats = { totalReports: 0, pendingReports: 0 };

        // Handle results based on what was fetched
        if (promises.length === 2) {
          [reportsData, statsData] = results;
        } else if (promises.length === 1) {
          // Only one request was made, determine which one
          if (getCachedStats()) {
            reportsData = results[0];
            statsData = getCachedStats()!;
          } else {
            statsData = results[0];
            reportsData = getCachedReports(cacheKey) || [];
          }
        }

        // Cache the results
        setCachedReports(cacheKey, reportsData);
        setCachedStats(statsData);

        setState((prev) => ({
          ...prev,
          reports: reportsData,
          stats: statsData,
          total: statsData.totalReports,
          loading: false,
          currentPage: page,
          pageSize,
          selectedStatus: status,
        }));
      } catch (error: any) {
        console.error("Error loading reports:", error);
        const errorMessage = error?.message || "Failed to load reports";
        setState((prev) => ({
          ...prev,
          error: errorMessage,
          loading: false,
        }));
        message.error(errorMessage);
      } finally {
        requestInFlightRef.current = false;
      }
    },
    [
      state.currentPage,
      state.pageSize,
      getCacheKey,
      getCachedReports,
      getCachedStats,
      setCachedReports,
      setCachedStats,
    ]
  );

  // Effect to handle debounced filter changes
  useEffect(() => {
    if (
      debouncedFilterStatus !== undefined ||
      debouncedFilterStatus === undefined
    ) {
      setState((prev) => ({
        ...prev,
        selectedStatus: debouncedFilterStatus,
        currentPage: 1,
      }));
      fetchReports(1, state.pageSize, debouncedFilterStatus);
    }
  }, [debouncedFilterStatus, state.pageSize]); // eslint-disable-line react-hooks/exhaustive-deps

  // Public API methods
  const filterByStatus = useCallback((status?: ReportStatus) => {
    setFilterStatus(status);
  }, []);

  const changePage = useCallback(
    (page: number, pageSize?: number) => {
      const newPageSize = pageSize || state.pageSize;
      setState((prev) => ({
        ...prev,
        currentPage: page,
        pageSize: newPageSize,
      }));
      fetchReports(page, newPageSize, state.selectedStatus);
    },
    [fetchReports, state.pageSize, state.selectedStatus]
  );

  const refresh = useCallback(() => {
    // Clear cache and force refresh
    reportsCache.clear();
    statsCache.data = null;
    fetchReports(state.currentPage, state.pageSize, state.selectedStatus, true);
  }, [fetchReports, state.currentPage, state.pageSize, state.selectedStatus]);

  // Update report action
  const updateReport = useCallback(
    async (reportId: string, updateData: any) => {
      try {
        setState((prev) => ({ ...prev, actionLoading: true }));
        await reportApi.update(reportId, updateData);

        // Clear cache and refresh
        reportsCache.clear();
        statsCache.data = null;
        await fetchReports(
          state.currentPage,
          state.pageSize,
          state.selectedStatus,
          true
        );

        message.success("Report updated successfully");
      } catch (error: any) {
        console.error("Error updating report:", error);
        message.error(error?.message || "Failed to update report");
      } finally {
        setState((prev) => ({ ...prev, actionLoading: false }));
      }
    },
    [fetchReports, state.currentPage, state.pageSize, state.selectedStatus]
  );

  // Delete report action
  const deleteReport = useCallback(
    async (reportId: string) => {
      try {
        setState((prev) => ({ ...prev, actionLoading: true }));
        await reportApi.delete(reportId);

        // Clear cache and refresh
        reportsCache.clear();
        statsCache.data = null;
        await fetchReports(
          state.currentPage,
          state.pageSize,
          state.selectedStatus,
          true
        );

        message.success("Report deleted successfully");
      } catch (error: any) {
        console.error("Error deleting report:", error);
        message.error(error?.message || "Failed to delete report");
      } finally {
        setState((prev) => ({ ...prev, actionLoading: false }));
      }
    },
    [fetchReports, state.currentPage, state.pageSize, state.selectedStatus]
  );

  // Initial load
  useEffect(() => {
    fetchReports();
  }, []); // Only run once on mount

  return {
    // Data
    reports: state.reports,
    stats: state.stats,

    // State
    loading: state.loading,
    actionLoading: state.actionLoading,
    error: state.error,

    // Pagination
    pagination: {
      current: state.currentPage,
      pageSize: state.pageSize,
      total: state.total,
      showSizeChanger: true,
      showQuickJumper: true,
      showTotal: (total: number, range: [number, number]) =>
        `${range[0]}-${range[1]} of ${total} reports`,
    },

    // Filters
    selectedStatus: state.selectedStatus,

    // Actions
    filterByStatus,
    changePage,
    refresh,
    updateReport,
    deleteReport,
  };
};
