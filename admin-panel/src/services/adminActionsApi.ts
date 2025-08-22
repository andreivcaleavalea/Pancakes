import { api } from "./api"; // Import the shared axios instance

export interface BanUserRequest {
  userId: string;
  reason: string;
  expiresAt?: string; // ISO date string, null for permanent ban
}

export interface BanResponse {
  id: string;
  userId: string;
  reason: string;
  bannedAt: string;
  bannedBy: string;
  expiresAt?: string;
  isActive: boolean;
}

export const adminActionsApi = {
  // Ban a user
  banUser: async (banData: BanUserRequest): Promise<BanResponse> => {
    const response = await api.post("/usermanagement/ban", banData);
    return response.data.data; // AdminService wraps responses in ApiResponse format
  },

  // Unban a user
  unbanUser: async (userId: string, reason: string): Promise<BanResponse> => {
    const response = await api.post("/usermanagement/unban", {
      userId,
      reason,
    });
    return response.data.data;
  },

  // Soft delete a blog post (mark as deleted)
  deleteBlogPost: async (
    blogPostId: string,
    reason: string = "Content violation"
  ): Promise<void> => {
    await api.delete(`/blogmanagement/posts/${blogPostId}`, {
      data: { blogPostId, reason },
    });
  },

  // Soft delete a comment (mark as deleted) - Note: This might need to be implemented in AdminService
  deleteComment: async (commentId: string): Promise<void> => {
    // TODO: This endpoint might need to be implemented in AdminService
    // For now, we'll throw an error to indicate it's not available
    throw new Error(
      `Comment deletion not yet implemented through AdminService (commentId: ${commentId})`
    );
  },

  // Get user ban status - Note: This might need to be implemented in AdminService
  getUserBanStatus: async (
    _userId: string
  ): Promise<{ isBanned: boolean; activeBan?: BanResponse }> => {
    try {
      // Mark parameter as used to satisfy strict TS checks
      void _userId;
      // TODO: This endpoint might need to be implemented in AdminService
      // For now, we'll return false to indicate user is not banned
      return { isBanned: false };
    } catch (error: any) {
      return { isBanned: false };
    }
  },
};
