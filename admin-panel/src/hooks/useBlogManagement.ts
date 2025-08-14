import { useCallback, useEffect, useReducer } from "react";
import { message } from "antd";
import { adminApi } from "../services/api";
import type { BlogPost, BlogPostSearchRequest, PagedResponse } from "../types";
import { useDebounce } from "./useDebounce";
import { useApiCache } from "./useApiCache";

// Constants for timing configurations
const DEBOUNCE_DELAY_MS = 500; // Delay for search input debouncing
const API_COMMIT_DELAY_MS = 500; // Delay to ensure backend has committed changes

// State interface for the blog management reducer
interface BlogManagementState {
  blogs: PagedResponse<BlogPost>;
  loading: boolean;
  actionLoading: boolean;
  error: string | null;
  searchTerm: string;
  searchParams: BlogPostSearchRequest;
}

// Action types for the reducer
type BlogManagementAction =
  | { type: "SET_LOADING"; payload: boolean }
  | { type: "SET_ACTION_LOADING"; payload: boolean }
  | { type: "SET_ERROR"; payload: string | null }
  | { type: "SET_BLOGS"; payload: PagedResponse<BlogPost> }
  | { type: "SET_SEARCH_TERM"; payload: string }
  | { type: "SET_SEARCH_PARAMS"; payload: BlogPostSearchRequest }
  | { type: "UPDATE_SEARCH_PARAMS"; payload: Partial<BlogPostSearchRequest> }
  | { type: "RESET_TO_PAGE_ONE" };

// Initial state
const initialState: BlogManagementState = {
  blogs: {
    data: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 0,
    hasNext: false,
    hasPrevious: false,
  },
  loading: false,
  actionLoading: false,
  error: null,
  searchTerm: "",
  searchParams: {
    page: 1,
    pageSize: 20,
    sortBy: "createdAt",
    sortOrder: "desc",
  },
};

// Reducer function
const blogManagementReducer = (
  state: BlogManagementState,
  action: BlogManagementAction
): BlogManagementState => {
  switch (action.type) {
    case "SET_LOADING":
      return { ...state, loading: action.payload };
    case "SET_ACTION_LOADING":
      return { ...state, actionLoading: action.payload };
    case "SET_ERROR":
      return { ...state, error: action.payload };
    case "SET_BLOGS":
      return { ...state, blogs: action.payload };
    case "SET_SEARCH_TERM":
      return { ...state, searchTerm: action.payload };
    case "SET_SEARCH_PARAMS":
      return { ...state, searchParams: action.payload };
    case "UPDATE_SEARCH_PARAMS":
      return {
        ...state,
        searchParams: { ...state.searchParams, ...action.payload },
      };
    case "RESET_TO_PAGE_ONE":
      return {
        ...state,
        searchParams: { ...state.searchParams, page: 1 },
      };
    default:
      return state;
  }
};

export const useBlogManagement = () => {
  // Initialize state with useReducer
  const [state, dispatch] = useReducer(blogManagementReducer, initialState);

  // Debounced search term
  const debouncedSearchTerm = useDebounce(state.searchTerm, DEBOUNCE_DELAY_MS);

  // Initialize cache hook
  const { getCacheBustingParams } = useApiCache();

  const fetchBlogs = useCallback(
    async (params: BlogPostSearchRequest = state.searchParams) => {
      dispatch({ type: "SET_LOADING", payload: true });
      dispatch({ type: "SET_ERROR", payload: null });

      try {
        const response = await adminApi.getBlogPosts(params);
        if (response.success) {
          dispatch({ type: "SET_BLOGS", payload: response.data });
        } else {
          dispatch({ type: "SET_ERROR", payload: response.message });
          message.error(response.message);
        }
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to fetch blog posts";
        dispatch({ type: "SET_ERROR", payload: errorMessage });
        message.error(errorMessage);
      } finally {
        dispatch({ type: "SET_LOADING", payload: false });
      }
    },
    [state.searchParams]
  );

  const search = useCallback(
    (searchTermValue: string) => {
      dispatch({ type: "SET_SEARCH_TERM", payload: searchTermValue });
      // Reset to first page when searching
      if (state.searchParams.page !== 1) {
        dispatch({ type: "RESET_TO_PAGE_ONE" });
      }
    },
    [state.searchParams.page]
  );

  const filterByStatus = useCallback(
    (status?: number) => {
      const newParams = {
        ...state.searchParams,
        status,
        page: 1,
      };
      dispatch({ type: "SET_SEARCH_PARAMS", payload: newParams });
      fetchBlogs(newParams);
    },
    [state.searchParams, fetchBlogs]
  );

  const filterByAuthor = useCallback(
    (authorId?: string) => {
      const newParams = {
        ...state.searchParams,
        authorId,
        page: 1,
      };
      dispatch({ type: "SET_SEARCH_PARAMS", payload: newParams });
      fetchBlogs(newParams);
    },
    [state.searchParams, fetchBlogs]
  );

  const changePage = useCallback(
    (page: number, pageSize?: number) => {
      const newParams = {
        ...state.searchParams,
        page,
        pageSize: pageSize || state.searchParams.pageSize,
      };
      dispatch({ type: "SET_SEARCH_PARAMS", payload: newParams });
      fetchBlogs(newParams);
    },
    [state.searchParams, fetchBlogs]
  );

  const deleteBlogPost = useCallback(
    async (blogPostId: string, reason: string) => {
      dispatch({ type: "SET_ACTION_LOADING", payload: true });

      try {
        const response = await adminApi.deleteBlogPost(blogPostId, reason);
        if (response.success) {
          message.success("Blog post deleted successfully");

          // Add a small delay to ensure backend has committed the changes
          await new Promise((resolve) =>
            setTimeout(resolve, API_COMMIT_DELAY_MS)
          );

          // Force refresh with current search params and cache busting
          await fetchBlogs({
            ...state.searchParams,
            ...getCacheBustingParams(true),
          });
          return true;
        } else {
          message.error(response.message);
          return false;
        }
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to delete blog post";
        message.error(errorMessage);
        return false;
      } finally {
        dispatch({ type: "SET_ACTION_LOADING", payload: false });
      }
    },
    [fetchBlogs, state.searchParams, getCacheBustingParams]
  );

  const updateBlogStatus = useCallback(
    async (blogPostId: string, status: number, reason: string) => {
      dispatch({ type: "SET_ACTION_LOADING", payload: true });

      try {
        const response = await adminApi.updateBlogPostStatus(
          blogPostId,
          status,
          reason
        );
        if (response.success) {
          message.success("Blog post status updated successfully");

          // Add a small delay to ensure backend has committed the changes
          await new Promise((resolve) =>
            setTimeout(resolve, API_COMMIT_DELAY_MS)
          );

          // Force refresh with current search params and cache busting
          await fetchBlogs({
            ...state.searchParams,
            ...getCacheBustingParams(true),
          });
          return true;
        } else {
          message.error(response.message);
          return false;
        }
      } catch (err) {
        const errorMessage =
          err instanceof Error
            ? err.message
            : "Failed to update blog post status";
        message.error(errorMessage);
        return false;
      } finally {
        dispatch({ type: "SET_ACTION_LOADING", payload: false });
      }
    },
    [fetchBlogs, state.searchParams, getCacheBustingParams]
  );

  const refresh = useCallback(() => {
    // Add timestamp to force fresh data on manual refresh
    const refreshParams = {
      ...state.searchParams,
      ...getCacheBustingParams(true),
    };
    fetchBlogs(refreshParams);
  }, [fetchBlogs, state.searchParams, getCacheBustingParams]);

  // Load initial data
  useEffect(() => {
    fetchBlogs();
  }, []); // Remove fetchBlogs from dependency to avoid infinite loop

  // Effect to handle debounced search
  useEffect(() => {
    const searchParamsWithTerm = {
      ...state.searchParams,
      search: debouncedSearchTerm || undefined,
      page: 1, // Reset to first page when search term changes
    };
    dispatch({ type: "SET_SEARCH_PARAMS", payload: searchParamsWithTerm });
    fetchBlogs(searchParamsWithTerm);
  }, [debouncedSearchTerm]); // Only depend on debounced search term

  return {
    blogs: state.blogs.data,
    pagination: {
      current: state.blogs.page,
      pageSize: state.blogs.pageSize,
      total: state.blogs.totalCount,
      showSizeChanger: true,
      showQuickJumper: true,
      showTotal: (total: number, range: [number, number]) =>
        `${range[0]}-${range[1]} of ${total} blog posts`,
    },
    loading: state.loading,
    actionLoading: state.actionLoading,
    error: state.error,
    searchParams: state.searchParams,
    searchTerm: state.searchTerm, // Expose search term for UI components
    search,
    filterByStatus,
    filterByAuthor,
    changePage,
    deleteBlogPost,
    updateBlogStatus,
    refresh,
  };
};
