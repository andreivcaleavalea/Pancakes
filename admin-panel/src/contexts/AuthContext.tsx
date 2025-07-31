import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { adminApi, AdminUser } from '../services/api'

interface AuthContextType {
  user: AdminUser | null
  login: (email: string, password: string) => Promise<void>
  logout: () => void
  isLoading: boolean
  isAuthenticated: boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

interface AuthProviderProps {
  children: ReactNode
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<AdminUser | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const token = localStorage.getItem('adminToken')
    if (token) {
      adminApi.setAuthToken(token)
      validateToken()
    } else {
      setIsLoading(false)
    }
  }, [])

  const validateToken = async () => {
    try {
      const response = await adminApi.getCurrentAdmin()
      setUser(response.data)
    } catch (error) {
      localStorage.removeItem('adminToken')
      adminApi.setAuthToken('')
    } finally {
      setIsLoading(false)
    }
  }

  const login = async (email: string, password: string) => {
    try {
      const response = await adminApi.login(email, password)
      const { token, adminUser } = response.data
      
      localStorage.setItem('adminToken', token)
      adminApi.setAuthToken(token)
      setUser(adminUser)
    } catch (error) {
      // Provide user-friendly error messages
      if (error instanceof Error) {
        throw new Error(error.message);
      } else if (typeof error === 'object' && error !== null && 'response' in error) {
        const axiosError = error as any;
        if (axiosError.response?.data?.message) {
          throw new Error(axiosError.response.data.message);
        }
      }
      
      throw new Error('Login failed. Please check your connection and try again.');
    }
  }

  const logout = () => {
    localStorage.removeItem('adminToken')
    adminApi.setAuthToken('')
    setUser(null)
  }

  const value = {
    user,
    login,
    logout,
    isLoading,
    isAuthenticated: !!user
  }

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}