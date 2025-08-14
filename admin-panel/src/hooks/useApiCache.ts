import { useCallback, useRef } from "react";

/**
 * Custom hook for managing API cache busting and request optimization
 */
export const useApiCache = () => {
  const lastRequestTimeRef = useRef<number>(0);
  const cacheKeyRef = useRef<string>("");

  /**
   * Creates cache busting parameters for API requests
   * @param forceRefresh - Whether to force a cache refresh
   * @returns Object with cache busting timestamp if needed
   */
  const getCacheBustingParams = useCallback((forceRefresh: boolean = false) => {
    if (forceRefresh) {
      return { _timestamp: Date.now() };
    }
    return {};
  }, []);

  /**
   * Generates a cache key for request parameters
   * @param params - The request parameters to generate key from
   * @returns A string cache key
   */
  const generateCacheKey = useCallback((params: Record<string, any>) => {
    return JSON.stringify(params);
  }, []);

  /**
   * Checks if we should skip the request based on cache key
   * @param params - The request parameters
   * @param minInterval - Minimum interval between requests in ms (default 100ms)
   * @returns true if request should be skipped
   */
  const shouldSkipRequest = useCallback(
    (params: Record<string, any>, minInterval: number = 100) => {
      const now = Date.now();
      const cacheKey = generateCacheKey(params);

      if (
        cacheKey === cacheKeyRef.current &&
        now - lastRequestTimeRef.current < minInterval
      ) {
        return true;
      }

      lastRequestTimeRef.current = now;
      cacheKeyRef.current = cacheKey;
      return false;
    },
    [generateCacheKey]
  );

  /**
   * Wraps an API call with cache busting and request optimization
   * @param apiCall - The API function to call
   * @param params - Parameters for the API call
   * @param options - Cache options
   */
  const wrapApiCall = useCallback(
    async <T>(
      apiCall: (params: any) => Promise<T>,
      params: Record<string, any>,
      options: {
        forceRefresh?: boolean;
        minInterval?: number;
        skipCacheCheck?: boolean;
      } = {}
    ) => {
      const {
        forceRefresh = false,
        minInterval = 100,
        skipCacheCheck = false,
      } = options;

      // Skip request if too frequent (unless cache check is disabled)
      if (!skipCacheCheck && shouldSkipRequest(params, minInterval)) {
        return null;
      }

      // Add cache busting if needed
      const finalParams = {
        ...params,
        ...getCacheBustingParams(forceRefresh),
      };

      return await apiCall(finalParams);
    },
    [getCacheBustingParams, shouldSkipRequest]
  );

  return {
    getCacheBustingParams,
    generateCacheKey,
    shouldSkipRequest,
    wrapApiCall,
  };
};
