/**
 * Preloader utility for critical resources
 */
export class Preloader {
  private static loadedResources = new Set<string>();
  private static loadingPromises = new Map<string, Promise<void>>();

  /**
   * Preload an image and cache the result
   */
  static async preloadImage(src: string): Promise<void> {
    if (this.loadedResources.has(src)) {
      return Promise.resolve();
    }

    if (this.loadingPromises.has(src)) {
      return this.loadingPromises.get(src)!;
    }

    const promise = new Promise<void>((resolve, reject) => {
      const img = new Image();

      img.onload = () => {
        this.loadedResources.add(src);
        this.loadingPromises.delete(src);
        resolve();
      };

      img.onerror = () => {
        this.loadingPromises.delete(src);
        reject(new Error(`Failed to preload image: ${src}`));
      };

      img.src = src;
    });

    this.loadingPromises.set(src, promise);
    return promise;
  }

  /**
   * Preload multiple images in parallel
   */
  static async preloadImages(sources: string[]): Promise<void[]> {
    return Promise.all(sources.map((src) => this.preloadImage(src)));
  }

  /**
   * Preload critical route components
   */
  static preloadRoute(routeImporter: () => Promise<any>): Promise<any> {
    // This will start loading the component but not wait for it
    return routeImporter().catch((err) => {
      console.warn("Failed to preload route:", err);
    });
  }

  /**
   * Check if a resource is already loaded
   */
  static isLoaded(src: string): boolean {
    return this.loadedResources.has(src);
  }

  /**
   * Clear the preload cache (useful for testing or memory management)
   */
  static clearCache(): void {
    this.loadedResources.clear();
    this.loadingPromises.clear();
  }
}

import { useEffect } from "react";

/**
 * Hook to preload images when component mounts
 */

export const useImagePreloader = (images: string[]) => {
  useEffect(() => {
    if (images.length > 0) {
      Preloader.preloadImages(images).catch((err) => {
        console.warn("Failed to preload some images:", err);
      });
    }
  }, [images]);
};

export default Preloader;
