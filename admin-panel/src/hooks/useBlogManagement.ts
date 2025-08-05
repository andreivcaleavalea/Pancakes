import { useState, useCallback, useEffect } from 'react'
import { message } from 'antd'
import { adminApi } from '../services/api'
import type { BlogPost, BlogPostSearchRequest, PagedResponse } from '../types'

export const useBlogManagement = () => {
  const [blogs, setBlogs] = useState<PagedResponse<BlogPost>>({
    data: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 0,
    hasNext: false,
    hasPrevious: false
  })
  
  const [loading, setLoading] = useState(false)
  const [actionLoading, setActionLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  
  // Search parameters
  const [searchParams, setSearchParams] = useState<BlogPostSearchRequest>({
    page: 1,
    pageSize: 20,
    sortBy: 'createdAt',
    sortOrder: 'desc'
  })

  const fetchBlogs = useCallback(async (params: BlogPostSearchRequest = searchParams) => {
    setLoading(true)
    setError(null)
    
    try {
      const response = await adminApi.getBlogPosts(params)
      if (response.success) {
        setBlogs(response.data)
      } else {
        setError(response.message)
        message.error(response.message)
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to fetch blog posts'
      setError(errorMessage)
      message.error(errorMessage)
    } finally {
      setLoading(false)
    }
  }, [searchParams])

  const search = useCallback((searchTerm: string) => {
    const newParams = { 
      ...searchParams, 
      search: searchTerm, 
      page: 1 
    }
    setSearchParams(newParams)
    fetchBlogs(newParams)
  }, [searchParams, fetchBlogs])

  const filterByStatus = useCallback((status?: number) => {
    const newParams = { 
      ...searchParams, 
      status, 
      page: 1 
    }
    setSearchParams(newParams)
    fetchBlogs(newParams)
  }, [searchParams, fetchBlogs])

  const filterByAuthor = useCallback((authorId?: string) => {
    const newParams = { 
      ...searchParams, 
      authorId, 
      page: 1 
    }
    setSearchParams(newParams)
    fetchBlogs(newParams)
  }, [searchParams, fetchBlogs])

  const changePage = useCallback((page: number, pageSize?: number) => {
    const newParams = { 
      ...searchParams, 
      page, 
      pageSize: pageSize || searchParams.pageSize 
    }
    setSearchParams(newParams)
    fetchBlogs(newParams)
  }, [searchParams, fetchBlogs])

  const deleteBlogPost = useCallback(async (blogPostId: string, reason: string) => {
    setActionLoading(true)
    
    try {
      const response = await adminApi.deleteBlogPost(blogPostId, reason)
      if (response.success) {
        message.success('Blog post deleted successfully')
        await fetchBlogs() // Refresh the list
        return true
      } else {
        message.error(response.message)
        return false
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to delete blog post'
      message.error(errorMessage)
      return false
    } finally {
      setActionLoading(false)
    }
  }, [fetchBlogs])

  const updateBlogStatus = useCallback(async (blogPostId: string, status: number, reason: string) => {
    setActionLoading(true)
    
    try {
      const response = await adminApi.updateBlogPostStatus(blogPostId, status, reason)
      if (response.success) {
        message.success('Blog post status updated successfully')
        await fetchBlogs() // Refresh the list
        return true
      } else {
        message.error(response.message)
        return false
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to update blog post status'
      message.error(errorMessage)
      return false
    } finally {
      setActionLoading(false)
    }
  }, [fetchBlogs])

  const refresh = useCallback(() => {
    fetchBlogs()
  }, [fetchBlogs])

  // Load initial data
  useEffect(() => {
    fetchBlogs()
  }, []) // Remove fetchBlogs from dependency to avoid infinite loop

  return {
    blogs: blogs.data,
    pagination: {
      current: blogs.page,
      pageSize: blogs.pageSize,
      total: blogs.totalCount,
      showSizeChanger: true,
      showQuickJumper: true,
      showTotal: (total: number, range: [number, number]) => 
        `${range[0]}-${range[1]} of ${total} blog posts`
    },
    loading,
    actionLoading,
    error,
    searchParams,
    search,
    filterByStatus,
    filterByAuthor,
    changePage,
    deleteBlogPost,
    updateBlogStatus,
    refresh
  }
}
