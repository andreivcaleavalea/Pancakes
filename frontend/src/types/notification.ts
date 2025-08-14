export interface Notification {
  id: string;
  userId: string;
  type: string;
  title: string;
  message: string;
  blogTitle?: string;
  blogId?: string;
  reason: string;
  source: string;
  additionalData?: string;
  isRead: boolean;
  createdAt: string;
  readAt?: string;
}

export interface CreateNotificationDto {
  userId: string;
  type: string;
  title: string;
  message: string;
  blogTitle?: string;
  blogId?: string;
  reason: string;
  source: string;
  additionalData?: string;
}

export interface NotificationListResponse {
  notifications: Notification[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface UnreadCountResponse {
  unreadCount: number;
}

