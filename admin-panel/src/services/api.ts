import axios from 'axios'
import type { 
  AdminUser, 
  LoginResponse, 
  ApiResponse, 
  PagedResponse, 
  DashboardStats, 
  UserOverview, 
  ContentFlag 
} from '../types'
import { API_CONFIG } from '../constants'

// Create axios instance
const api = axios.create({
  baseURL: `${API_CONFIG.BASE_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: API_CONFIG.TIMEOUT,
  withCredentials: true, 
})

// Add response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Handle different error types
    if (error.code === 'ECONNABORTED') {
      throw new Error('Request timeout - please check your connection and try again');
    } else if (error.code === 'ERR_NETWORK') {
      throw new Error('Network error - please check if the admin service is running');
    } else if (error.response?.status === 401) {
      throw new Error('Invalid credentials');
    } else if (error.response?.status === 403) {
      throw new Error('Access denied');
    } else if (error.response?.status >= 500) {
      throw new Error('Server error - please try again later');
    }
    
    return Promise.reject(error);
  }
);

// API Service
class AdminApiService {
  // Auth endpoints
  async login(email: string, password: string): Promise<ApiResponse<LoginResponse>> {
    const response = await api.post('/adminauth/login', { email, password })
    return response.data
  }

  async logout(): Promise<ApiResponse<object>> {
    const response = await api.post('/adminauth/logout')
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

export type { 
  AdminUser, 
  LoginResponse, 
  ApiResponse, 
  PagedResponse, 
  DashboardStats, 
  UserOverview, 
  ContentFlag 
} from '../types'