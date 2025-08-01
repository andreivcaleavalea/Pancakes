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

export interface UserOverview {
  id: string
  name: string
  email: string
  provider: string
  createdAt: string
  lastLoginAt: string
  totalBlogPosts: number
  totalComments: number
  reportsCount: number
  isBanned: boolean
  currentBanReason?: string
  currentBanExpiresAt?: string
  currentBannedAt?: string
  currentBannedBy?: string
  totalBansCount: number
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
