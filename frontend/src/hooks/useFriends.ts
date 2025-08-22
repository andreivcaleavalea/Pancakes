import { useState, useEffect, useCallback } from "react";
import { authenticatedBlogRequest } from "@/utils/blogApi";
import { friendshipApi } from "@/services/friendshipApi";
import type {
  Friend,
  FriendRequest,
  AvailableUser,
} from "@/services/friendshipApi";
import type { BlogPost, PaginatedResult } from "@/types/blog";

interface PaginationInfo {
  currentPage: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
}

interface UsePaginatedFriendsPostsResult {
  data: BlogPost[];
  pagination: PaginationInfo | null;
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export function usePaginatedFriendsPosts(
  page: number = 1,
  pageSize: number = 10,
  enabled: boolean = true
): UsePaginatedFriendsPostsResult {
  const [data, setData] = useState<BlogPost[]>([]);
  const [pagination, setPagination] = useState<PaginationInfo | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchPosts = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      console.log(
        `[Friends Posts] Fetching friends posts - page: ${page}, pageSize: ${pageSize}`
      );

      const response = await authenticatedBlogRequest<
        PaginatedResult<BlogPost>
      >(`/api/blogposts/friends?page=${page}&pageSize=${pageSize}`);

      console.log(`[Friends Posts] API Response:`, response);
      console.log(`[Friends Posts] Posts data:`, response.data);
      console.log(`[Friends Posts] Pagination:`, response.pagination);

      setData(response.data || []);

      if (response.pagination) {
        setPagination({
          currentPage: response.pagination.currentPage,
          totalPages: response.pagination.totalPages,
          totalCount: response.pagination.totalItems,
          pageSize: response.pagination.pageSize,
        });
      }
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to fetch friends posts";
      setError(errorMessage);
      console.error("Error fetching friends posts:", err);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => {
    if (!enabled) {
      return;
    }
    fetchPosts();
  }, [fetchPosts, enabled]);

  return {
    data,
    pagination,
    loading,
    error,
    refetch: fetchPosts,
  };
}

export function useFriends(enabled: boolean = true) {
  const [friends, setFriends] = useState<Friend[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchFriends = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const friendsData = await friendshipApi.getFriends();
      setFriends(friendsData);
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to fetch friends";
      setError(errorMessage);
      console.error("Error fetching friends:", err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!enabled) {
      return;
    }
    fetchFriends();
  }, [fetchFriends, enabled]);

  return {
    friends,
    loading,
    error,
    refetch: fetchFriends,
  };
}

export function useFriendRequests(enabled: boolean = true) {
  const [requests, setRequests] = useState<FriendRequest[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchRequests = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const requestsData = await friendshipApi.getPendingRequests();
      setRequests(requestsData);
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to fetch friend requests";
      setError(errorMessage);
      console.error("Error fetching friend requests:", err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!enabled) {
      return;
    }
    fetchRequests();
  }, [fetchRequests, enabled]);

  return {
    requests,
    loading,
    error,
    refetch: fetchRequests,
  };
}

export function useAvailableUsers(enabled: boolean = true) {
  const [users, setUsers] = useState<AvailableUser[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchUsers = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const usersData = await friendshipApi.getAvailableUsers();
      setUsers(usersData);
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to fetch available users";
      setError(errorMessage);
      console.error("Error fetching available users:", err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!enabled) {
      return;
    }
    fetchUsers();
  }, [fetchUsers, enabled]);

  return {
    users,
    loading,
    error,
    refetch: fetchUsers,
  };
}
