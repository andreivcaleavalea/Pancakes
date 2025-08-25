/**
 * Memory optimization utilities
 */

/**
 * Debounce function to limit the rate of function calls
 */
export function debounce<T extends (...args: any[]) => any>(
  func: T,
  wait: number,
  immediate = false
): (...args: Parameters<T>) => void {
  let timeout: NodeJS.Timeout | null = null;

  return function executedFunction(...args: Parameters<T>) {
    const later = () => {
      timeout = null;
      if (!immediate) func(...args);
    };

    const callNow = immediate && !timeout;

    if (timeout) clearTimeout(timeout);
    timeout = setTimeout(later, wait);

    if (callNow) func(...args);
  };
}

/**
 * Throttle function to limit function execution frequency
 */
export function throttle<T extends (...args: any[]) => any>(
  func: T,
  limit: number
): (...args: Parameters<T>) => void {
  let inThrottle: boolean;

  return function executedFunction(...args: Parameters<T>) {
    if (!inThrottle) {
      func.apply(this, args);
      inThrottle = true;
      setTimeout(() => (inThrottle = false), limit);
    }
  };
}

/**
 * Simple LRU cache implementation for API responses
 */
export class LRUCache<K, V> {
  private max: number;
  private cache: Map<K, V>;

  constructor(max = 50) {
    this.max = max;
    this.cache = new Map();
  }

  get(key: K): V | undefined {
    const item = this.cache.get(key);
    if (item !== undefined) {
      // Move to end (most recently used)
      this.cache.delete(key);
      this.cache.set(key, item);
    }
    return item;
  }

  set(key: K, value: V): void {
    if (this.cache.has(key)) {
      // Update existing
      this.cache.delete(key);
    } else if (this.cache.size >= this.max) {
      // Remove least recently used
      const firstKey = this.cache.keys().next().value;
      this.cache.delete(firstKey);
    }
    this.cache.set(key, value);
  }

  has(key: K): boolean {
    return this.cache.has(key);
  }

  clear(): void {
    this.cache.clear();
  }

  size(): number {
    return this.cache.size;
  }
}

/**
 * Memory-conscious API cache
 */
export const apiCache = new LRUCache<string, any>(100);

/**
 * Cache key generator for API requests
 */
export function generateCacheKey(
  url: string,
  params?: Record<string, any>
): string {
  const baseKey = url;
  if (!params) return baseKey;

  const sortedParams = Object.keys(params)
    .sort()
    .map((key) => `${key}=${JSON.stringify(params[key])}`)
    .join("&");

  return `${baseKey}?${sortedParams}`;
}

/**
 * Cleanup utility for event listeners and timers
 */
export class CleanupManager {
  private cleanupFunctions: (() => void)[] = [];

  add(cleanup: () => void): void {
    this.cleanupFunctions.push(cleanup);
  }

  addEventListener(
    element: EventTarget,
    event: string,
    handler: EventListener,
    options?: AddEventListenerOptions
  ): void {
    element.addEventListener(event, handler, options);
    this.add(() => element.removeEventListener(event, handler, options));
  }

  addInterval(callback: () => void, ms: number): void {
    const intervalId = setInterval(callback, ms);
    this.add(() => clearInterval(intervalId));
  }

  addTimeout(callback: () => void, ms: number): void {
    const timeoutId = setTimeout(callback, ms);
    this.add(() => clearTimeout(timeoutId));
  }

  cleanup(): void {
    this.cleanupFunctions.forEach((cleanup) => {
      try {
        cleanup();
      } catch (error) {
        console.warn("Cleanup error:", error);
      }
    });
    this.cleanupFunctions = [];
  }
}

/**
 * React hook for cleanup management
 */
import { useEffect, useRef } from "react";

export function useCleanup() {
  const cleanupManager = useRef(new CleanupManager());

  useEffect(() => {
    return () => {
      cleanupManager.current.cleanup();
    };
  }, []);

  return cleanupManager.current;
}

export default {
  debounce,
  throttle,
  LRUCache,
  apiCache,
  generateCacheKey,
  CleanupManager,
  useCleanup,
};
