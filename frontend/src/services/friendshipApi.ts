import { authenticatedFetch } from "@/lib/api";

export interface Friend {
  userId: string;
  name: string;
  image?: string;
  friendsSince: string; // ISO date string
}

export interface FriendRequest {
  id: string; // Guid as string
  senderId: string;
  senderName: string;
  senderImage?: string;
  createdAt: string; // ISO date string
}

export interface AvailableUser {
  id: string;
  name: string;
  image?: string;
  email?: string;
  bio?: string;
}

export interface FriendshipDto {
  id: string; // Guid as string
  senderId: string;
  receiverId: string;
  status: string;
  createdAt: string; // ISO date string
  acceptedAt?: string; // ISO date string
}

class FriendshipApi {
  async getFriends(): Promise<Friend[]> {
    const response = await authenticatedFetch("/api/friendships/friends");

    if (!response.data) {
      throw new Error(response.error || "Failed to fetch friends");
    }

    return response.data as Friend[];
  }

  async getPendingRequests(): Promise<FriendRequest[]> {
    const response = await authenticatedFetch("/api/friendships/requests");

    if (!response.data) {
      throw new Error(response.error || "Failed to fetch friend requests");
    }

    return response.data as FriendRequest[];
  }

  async getAvailableUsers(): Promise<AvailableUser[]> {
    const response = await authenticatedFetch(
      "/api/friendships/available-users"
    );

    if (!response.data) {
      throw new Error(response.error || "Failed to fetch available users");
    }

    return response.data as AvailableUser[];
  }

  async sendFriendRequest(receiverId: string): Promise<FriendshipDto> {
    const response = await authenticatedFetch("/api/friendships/send-request", {
      method: "POST",
      body: JSON.stringify({ receiverId }),
    });

    if (!response.data) {
      throw new Error(response.error || "Failed to send friend request");
    }

    return response.data as FriendshipDto;
  }

  async acceptFriendRequest(friendshipId: string): Promise<FriendshipDto> {
    const response = await authenticatedFetch(
      `/api/friendships/${friendshipId}/accept`,
      {
        method: "POST",
      }
    );

    if (!response.data) {
      throw new Error(response.error || "Failed to accept friend request");
    }

    return response.data as FriendshipDto;
  }

  async rejectFriendRequest(friendshipId: string): Promise<FriendshipDto> {
    const response = await authenticatedFetch(
      `/api/friendships/${friendshipId}/reject`,
      {
        method: "POST",
      }
    );

    if (!response.data) {
      throw new Error(response.error || "Failed to reject friend request");
    }

    return response.data as FriendshipDto;
  }

  async removeFriend(friendUserId: string): Promise<void> {
    const response = await authenticatedFetch(
      `/api/friendships/remove/${friendUserId}`,
      {
        method: "DELETE",
      }
    );

    if (response.error) {
      throw new Error(response.error || "Failed to remove friend");
    }
  }

  async checkFriendship(userId: string): Promise<boolean> {
    const response = await authenticatedFetch(
      `/api/friendships/check-friendship/${userId}`
    );

    if (!response.data) {
      throw new Error(response.error || "Failed to check friendship status");
    }

    return response.data as boolean;
  }
}

export const friendshipApi = new FriendshipApi();
