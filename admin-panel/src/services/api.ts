import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_ADMIN_API_URL || 'http://localhost:5002'

// Create axios instance
const api = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Types
export interface AdminUser {
  id: string
  email: string
  name: string
  adminLevel: number
  isActive: boolean
  requirePasswordChange: boolean
  twoFactorEnabled: boolean
  createdAt: string
  lastLoginAt: string
  roles: AdminRole[]
}

export interface AdminRole {
  id: string
  name: string
  description: string
  level: number
  permissions: string[]
  isActive: boolean
}

export interface LoginResponse {
  token: string
  adminUser: AdminUser
  expiresAt: string
  requirePasswordChange: boolean
  requireTwoFactor: boolean
}

export interface ApiResponse<T> {
  success: boolean
  message: string
  data: T
  errors: string[]
}

export interface PagedResponse<T> {
  data: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNext: boolean
  hasPrevious: boolean
}

export interface DashboardStats {
  userStats: {
    totalUsers: number
    activeUsers: number
    onlineUsers: number
    dailySignups: number
    weeklySignups: number
    monthlySignups: number
    growthRate: number
  }
  contentStats: {
    totalBlogPosts: number
    publishedBlogPosts: number
    draftBlogPosts: number
    blogPostsToday: number
    totalComments: number
    commentsToday: number
    averageRating: number
  }
  moderationStats: {
    totalReports: number
    pendingReports: number
    totalFlags: number
    pendingFlags: number
    bannedUsers: number
    deletedPosts: number
    deletedComments: number
  }
  systemStats: {
    cpuUsage: number
    memoryUsage: number
    diskUsage: number
    averageResponseTime: number
    errorsLastHour: number
  }
}

export interface UserOverview {
  id: string
  name: string
  email: string
  provider: string
  createdAt: string
  lastLoginAt: string
  isActive: boolean
  totalBlogPosts: number
  totalComments: number
  reportsCount: number
  isBanned: boolean
}

export interface ContentFlag {
  id: string
  contentType: string
  contentId: string
  flagType: string
  flaggedBy?: string
  autoDetected: boolean
  severity: number
  status: string
  reviewedBy?: string
  reviewedAt?: string
  reviewNotes?: string
  description: string
  createdAt: string
}

// API Service
class AdminApiService {
  private authToken = ''

  setAuthToken(token: string) {
    this.authToken = token
    if (token) {
      api.defaults.headers.common['Authorization'] = `Bearer ${token}`
    } else {
      delete api.defaults.headers.common['Authorization']
    }
  }

  // Auth endpoints
  async login(email: string, password: string): Promise<ApiResponse<LoginResponse>> {
    const response = await api.post('/adminauth/login', { email, password })
    return response.data
  }

  async getCurrentAdmin(): Promise<ApiResponse<AdminUser>> {
    const response = await api.get('/adminauth/me')
    return response.data
  }

  async validateToken(): Promise<ApiResponse<{ isValid: boolean }>> {
    const response = await api.post('/adminauth/validate')
    return response.data
  }

  // Dashboard endpoints
  async getDashboardStats(): Promise<ApiResponse<DashboardStats>> {
    const response = await api.get('/analytics/dashboard')
    return response.data
  }

  // User management endpoints
  async getUsers(page = 1, pageSize = 20, searchTerm = ''): Promise<ApiResponse<PagedResponse<UserOverview>>> {
    const response = await api.get('/usermanagement/search', {
      params: { page, pageSize, searchTerm }
    })
    return response.data
  }

  async getUserDetails(userId: string): Promise<ApiResponse<any>> {
    const response = await api.get(`/usermanagement/users/${userId}`)
    return response.data
  }

  async banUser(userId: string, reason: string, expiresAt?: string): Promise<ApiResponse<boolean>> {
    const response = await api.post('/usermanagement/ban', {
      userId,
      reason,
      expiresAt
    })
    return response.data
  }

  async unbanUser(userId: string, reason: string): Promise<ApiResponse<boolean>> {
    const response = await api.post('/usermanagement/unban', {
      userId,
      reason
    })
    return response.data
  }

  // Content moderation endpoints
  async getContentFlags(page = 1, pageSize = 20, status = '', contentType = ''): Promise<ApiResponse<PagedResponse<ContentFlag>>> {
    const response = await api.get('/contentmoderation/flags', {
      params: { page, pageSize, status, contentType }
    })
    return response.data
  }

  async getPendingFlags(): Promise<ApiResponse<ContentFlag[]>> {
    const response = await api.get('/contentmoderation/flags/pending')
    return response.data
  }

  async reviewFlag(flagId: string, action: string, reviewNotes: string): Promise<ApiResponse<boolean>> {
    const response = await api.post('/contentmoderation/flags/review', {
      flagId,
      action,
      reviewNotes
    })
    return response.data
  }

  // Analytics endpoints
  async getAnalyticsData(fromDate?: string, toDate?: string): Promise<ApiResponse<any>> {
    const response = await api.get('/analytics/detailed', {
      params: { fromDate, toDate }
    })
    return response.data
  }

  // System configuration endpoints
  async getSystemConfigurations(): Promise<ApiResponse<any[]>> {
    const response = await api.get('/system/configurations')
    return response.data
  }

  async updateConfiguration(key: string, value: string): Promise<ApiResponse<boolean>> {
    const response = await api.put('/system/configurations', {
      key,
      value
    })
    return response.data
  }
}

export const adminApi = new AdminApiService()

// Request interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('adminToken')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)