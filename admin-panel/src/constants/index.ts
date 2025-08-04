export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_ADMIN_API_URL || 'http://localhost:5002',
  TIMEOUT: 10000,
  RETRY_ATTEMPTS: 3,
} as const

export const ROUTES = {
  LOGIN: '/login',
  DASHBOARD: '/dashboard',
  USERS: '/users',
  CONTENT: '/content',
  ANALYTICS: '/analytics',
  SETTINGS: '/settings',
} as const

export const LOCAL_STORAGE_KEYS = {
  AUTH_TOKEN: 'adminToken',
  USER_PREFERENCES: 'adminUserPreferences',
} as const

export const PAGINATION = {
  DEFAULT_PAGE_SIZE: 20,
  PAGE_SIZE_OPTIONS: [10, 20, 50, 100],
} as const

export const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  INTERNAL_SERVER_ERROR: 500,
} as const
