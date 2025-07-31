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
