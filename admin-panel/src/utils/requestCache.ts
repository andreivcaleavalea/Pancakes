// Simple request cache and deduplication utility for admin panel
// Prevents duplicate API calls and provides caching for frequently accessed data

interface CacheEntry<T> {
  data: T;
  timestamp: number;
  expiresAt: number;
}

interface PendingRequest {
  promise: Promise<any>;
  timestamp: number;
}

export class RequestCache {
  private cache = new Map<string, CacheEntry<any>>();
  private pendingRequests = new Map<string, PendingRequest>();
  private defaultTTL: number;

  constructor(defaultTTL: number = 60000) {
    // 1 minute default
    this.defaultTTL = defaultTTL;
  }

  /**
   * Get data from cache if available and not expired
   */
  get<T>(key: string): T | null {
    const entry = this.cache.get(key);

    if (!entry) {
      return null;
    }

    if (Date.now() > entry.expiresAt) {
      this.cache.delete(key);
      return null;
    }

    return entry.data as T;
  }

  /**
   * Set data in cache with optional TTL
   */
  set<T>(key: string, data: T, ttl?: number): void {
    const expires = ttl || this.defaultTTL;
    const entry: CacheEntry<T> = {
      data,
      timestamp: Date.now(),
      expiresAt: Date.now() + expires,
    };

    this.cache.set(key, entry);
  }

  /**
   * Delete entry from cache
   */
  delete(key: string): void {
    this.cache.delete(key);
  }

  /**
   * Clear all cache entries
   */
  clear(): void {
    this.cache.clear();
    this.pendingRequests.clear();
  }

  /**
   * Clear expired entries
   */
  clearExpired(): void {
    const now = Date.now();

    for (const [key, entry] of this.cache.entries()) {
      if (now > entry.expiresAt) {
        this.cache.delete(key);
      }
    }

    // Clean up old pending requests (older than 30 seconds)
    for (const [key, request] of this.pendingRequests.entries()) {
      if (now - request.timestamp > 30000) {
        this.pendingRequests.delete(key);
      }
    }
  }

  /**
   * Execute a function with caching and deduplication
   * If the same request is already in flight, it returns the existing promise
   * If data is cached and fresh, it returns the cached data
   */
  async execute<T>(
    key: string,
    fetchFn: () => Promise<T>,
    options: {
      ttl?: number;
      force?: boolean;
      enableDeduplication?: boolean;
    } = {}
  ): Promise<T> {
    const { ttl, force = false, enableDeduplication = true } = options;

    // Check for cached data first (unless force refresh)
    if (!force) {
      const cached = this.get<T>(key);
      if (cached !== null) {
        return cached;
      }
    }

    // Check for pending request (deduplication)
    if (enableDeduplication && !force) {
      const pending = this.pendingRequests.get(key);
      if (pending) {
        return pending.promise as Promise<T>;
      }
    }

    // Execute the request
    const promise = fetchFn().then(
      (data) => {
        // Cache the successful result
        this.set(key, data, ttl);
        // Remove from pending requests
        this.pendingRequests.delete(key);
        return data;
      },
      (error) => {
        // Remove from pending requests on error
        this.pendingRequests.delete(key);
        throw error;
      }
    );

    // Track pending request
    if (enableDeduplication) {
      this.pendingRequests.set(key, {
        promise,
        timestamp: Date.now(),
      });
    }

    return promise;
  }

  /**
   * Invalidate cache entries by pattern
   */
  invalidatePattern(pattern: string | RegExp): void {
    const regex = typeof pattern === "string" ? new RegExp(pattern) : pattern;

    for (const key of this.cache.keys()) {
      if (regex.test(key)) {
        this.cache.delete(key);
      }
    }
  }

  /**
   * Get cache statistics
   */
  getStats() {
    const now = Date.now();
    let totalEntries = 0;
    let expiredEntries = 0;

    for (const entry of this.cache.values()) {
      totalEntries++;
      if (now > entry.expiresAt) {
        expiredEntries++;
      }
    }

    return {
      totalEntries,
      expiredEntries,
      pendingRequests: this.pendingRequests.size,
      hitRate:
        totalEntries > 0
          ? ((totalEntries - expiredEntries) / totalEntries) * 100
          : 0,
    };
  }
}

// Create global cache instances for different types of data
export const adminCache = new RequestCache(60000); // 1 minute for general admin data
export const userCache = new RequestCache(300000); // 5 minutes for user data
export const statsCache = new RequestCache(120000); // 2 minutes for statistics

// Utility functions for common cache operations
export const cacheUtils = {
  /**
   * Create a cache key from parameters
   */
  createKey: (
    prefix: string,
    ...params: (string | number | undefined)[]
  ): string => {
    return `${prefix}:${params.filter((p) => p !== undefined).join(":")}`;
  },

  /**
   * Invalidate all cache entries for a specific entity type
   */
  invalidateEntity: (entityType: string): void => {
    adminCache.invalidatePattern(`^${entityType}:`);
    userCache.invalidatePattern(`^${entityType}:`);
    statsCache.invalidatePattern(`^${entityType}:`);
  },

  /**
   * Clear all caches
   */
  clearAll: (): void => {
    adminCache.clear();
    userCache.clear();
    statsCache.clear();
  },
};

// Auto-cleanup expired entries every 5 minutes
setInterval(() => {
  adminCache.clearExpired();
  userCache.clearExpired();
  statsCache.clearExpired();
}, 300000);

export default RequestCache;
