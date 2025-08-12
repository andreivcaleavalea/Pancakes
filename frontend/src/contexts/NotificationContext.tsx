import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
} from "react";
import { notificationApi } from "../services/notificationApi";
import { useAuth } from "./AuthContext";

interface NotificationContextType {
  unreadCount: number;
  loading: boolean;
  error: string | null;
  markAsRead: (notificationId: string) => void;
  markAllAsRead: () => void;
  refresh: () => void;
  decrementCount: (amount?: number) => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(
  undefined
);

const REFRESH_INTERVAL = 30000; // 30 seconds
const CACHE_DURATION = 60000; // 1 minute

// Simple cache for notification count
let notificationCountCache: {
  count: number;
  timestamp: number;
} | null = null;

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const { isAuthenticated } = useAuth();
  const [unreadCount, setUnreadCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchNotificationCount = useCallback(
    async (force = false) => {
      if (!isAuthenticated) {
        setUnreadCount(0);
        setLoading(false);
        return;
      }

      // Check cache first (unless forced)
      if (
        !force &&
        notificationCountCache &&
        Date.now() - notificationCountCache.timestamp < CACHE_DURATION
      ) {
        setUnreadCount(notificationCountCache.count);
        return;
      }

      setLoading(true);
      setError(null);

      try {
        const response = await notificationApi.getUnreadCount();
        const count = response.unreadCount;

        // Update cache
        notificationCountCache = {
          count,
          timestamp: Date.now(),
        };

        setUnreadCount(count);
        setLoading(false);
      } catch (error) {
        const errorMessage =
          error instanceof Error
            ? error.message
            : "Failed to fetch notification count";

        setError(errorMessage);
        setLoading(false);
      }
    },
    [isAuthenticated]
  );

  const refresh = useCallback(() => {
    fetchNotificationCount(true);
  }, [fetchNotificationCount]);

  const decrementCount = useCallback((amount: number = 1) => {
    setUnreadCount((prev) => Math.max(0, prev - amount));
    // Also update the cache to keep it in sync
    if (notificationCountCache) {
      notificationCountCache.count = Math.max(
        0,
        notificationCountCache.count - amount
      );
      notificationCountCache.timestamp = Date.now();
    }
  }, []);

  const markAsRead = useCallback(
    async (notificationId: string) => {
      try {
        await notificationApi.markAsRead(notificationId);
        decrementCount(1);
      } catch (error) {
        console.error("Failed to mark notification as read:", error);
        // Refresh count in case of error to keep it accurate
        refresh();
      }
    },
    [decrementCount, refresh]
  );

  const markAllAsRead = useCallback(async () => {
    try {
      await notificationApi.markAllAsRead();
      setUnreadCount(0);
      // Update cache
      if (notificationCountCache) {
        notificationCountCache.count = 0;
        notificationCountCache.timestamp = Date.now();
      }
    } catch (error) {
      console.error("Failed to mark all notifications as read:", error);
      // Refresh count in case of error to keep it accurate
      refresh();
    }
  }, [refresh]);

  // Clear cache when user logs out
  useEffect(() => {
    if (!isAuthenticated) {
      notificationCountCache = null;
      setUnreadCount(0);
      setLoading(false);
      setError(null);
    }
  }, [isAuthenticated]);

  // Initial fetch when authenticated
  useEffect(() => {
    if (isAuthenticated) {
      fetchNotificationCount();
    }
  }, [fetchNotificationCount, isAuthenticated]);

  // Set up periodic refresh when authenticated
  useEffect(() => {
    if (!isAuthenticated) return;

    const interval = setInterval(() => {
      fetchNotificationCount();
    }, REFRESH_INTERVAL);

    return () => clearInterval(interval);
  }, [fetchNotificationCount, isAuthenticated]);

  // Refresh when page becomes visible (user comes back to tab)
  useEffect(() => {
    if (!isAuthenticated) return;

    const handleVisibilityChange = () => {
      if (!document.hidden) {
        fetchNotificationCount(true);
      }
    };

    document.addEventListener("visibilitychange", handleVisibilityChange);
    return () =>
      document.removeEventListener("visibilitychange", handleVisibilityChange);
  }, [fetchNotificationCount, isAuthenticated]);

  const value: NotificationContextType = {
    unreadCount,
    loading,
    error,
    markAsRead,
    markAllAsRead,
    refresh,
    decrementCount,
  };

  return (
    <NotificationContext.Provider value={value}>
      {children}
    </NotificationContext.Provider>
  );
};

export const useNotificationContext = () => {
  const context = useContext(NotificationContext);
  if (context === undefined) {
    throw new Error(
      "useNotificationContext must be used within a NotificationProvider"
    );
  }
  return context;
};
