import { authenticatedUserRequest } from "@/utils/userApi";
import type {
  Notification,
  CreateNotificationDto,
  UnreadCountResponse,
} from "@/types/notification";

const ENDPOINTS = {
  NOTIFICATIONS: "/api/notifications",
} as const;

export const notificationApi = {
  // Get user notifications with pagination
  getAll: async (
    page: number = 1,
    pageSize: number = 20
  ): Promise<Notification[]> => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    return authenticatedUserRequest<Notification[]>(
      `${ENDPOINTS.NOTIFICATIONS}?${params.toString()}`
    );
  },

  // Get unread notification count
  getUnreadCount: async (): Promise<UnreadCountResponse> => {
    return authenticatedUserRequest<UnreadCountResponse>(
      `${ENDPOINTS.NOTIFICATIONS}/unread-count`
    );
  },

  // Mark a notification as read
  markAsRead: async (notificationId: string): Promise<void> => {
    return authenticatedUserRequest<void>(
      `${ENDPOINTS.NOTIFICATIONS}/${notificationId}/mark-read`,
      {
        method: "PUT",
      }
    );
  },

  // Mark all notifications as read
  markAllAsRead: async (): Promise<void> => {
    return authenticatedUserRequest<void>(
      `${ENDPOINTS.NOTIFICATIONS}/mark-all-read`,
      {
        method: "PUT",
      }
    );
  },

  // Delete a notification
  delete: async (notificationId: string): Promise<void> => {
    return authenticatedUserRequest<void>(
      `${ENDPOINTS.NOTIFICATIONS}/${notificationId}`,
      {
        method: "DELETE",
      }
    );
  },

  // Create a notification (admin only - probably won't be used in frontend)
  create: async (
    notificationData: CreateNotificationDto
  ): Promise<Notification> => {
    return authenticatedUserRequest<Notification>(ENDPOINTS.NOTIFICATIONS, {
      method: "POST",
      body: JSON.stringify(notificationData),
    });
  },
};









