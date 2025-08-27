/**
 * Performance monitoring utilities
 */
export class PerformanceMonitor {
  private static timers = new Map<string, number>();
  private static marks = new Map<string, number>();

  /**
   * Start timing an operation
   */
  static startTimer(name: string): void {
    this.timers.set(name, performance.now());

    // Also create a performance mark for devtools
    if (typeof performance.mark === "function") {
      performance.mark(`${name}-start`);
    }
  }

  /**
   * End timing an operation and log the result
   */
  static endTimer(name: string, shouldLog = true): number {
    const startTime = this.timers.get(name);
    if (!startTime) {
      console.warn(`Timer "${name}" was not started`);
      return 0;
    }

    const duration = performance.now() - startTime;
    this.timers.delete(name);

    // Create performance mark and measure
    if (
      typeof performance.mark === "function" &&
      typeof performance.measure === "function"
    ) {
      performance.mark(`${name}-end`);
      performance.measure(name, `${name}-start`, `${name}-end`);
    }

    if (shouldLog) {
      console.log(`⚡ ${name}: ${duration.toFixed(2)}ms`);
    }

    return duration;
  }

  /**
   * Mark a specific point in time
   */
  static mark(name: string): void {
    this.marks.set(name, performance.now());

    if (typeof performance.mark === "function") {
      performance.mark(name);
    }
  }

  /**
   * Measure time between two marks
   */
  static measureBetween(
    name: string,
    startMark: string,
    endMark: string
  ): number {
    const startTime = this.marks.get(startMark);
    const endTime = this.marks.get(endMark);

    if (!startTime || !endTime) {
      console.warn(`Marks "${startMark}" or "${endMark}" not found`);
      return 0;
    }

    const duration = endTime - startTime;

    if (typeof performance.measure === "function") {
      performance.measure(name, startMark, endMark);
    }

    console.log(`📊 ${name}: ${duration.toFixed(2)}ms`);
    return duration;
  }

  /**
   * Monitor first contentful paint and largest contentful paint
   */
  static monitorWebVitals(): void {
    // First Contentful Paint
    if ("PerformanceObserver" in window) {
      try {
        const paintObserver = new PerformanceObserver((list) => {
          list.getEntries().forEach((entry) => {
            if (entry.name === "first-contentful-paint") {
              console.log(
                `🎨 First Contentful Paint: ${entry.startTime.toFixed(2)}ms`
              );
            }
          });
        });
        paintObserver.observe({ entryTypes: ["paint"] });

        // Largest Contentful Paint
        const lcpObserver = new PerformanceObserver((list) => {
          const entries = list.getEntries();
          const lastEntry = entries[entries.length - 1];
          console.log(
            `📏 Largest Contentful Paint: ${lastEntry.startTime.toFixed(2)}ms`
          );
        });
        lcpObserver.observe({ entryTypes: ["largest-contentful-paint"] });

        // Layout shift
        const clsObserver = new PerformanceObserver((list) => {
          let clsValue = 0;
          list.getEntries().forEach((entry: any) => {
            if (!entry.hadRecentInput) {
              clsValue += entry.value;
            }
          });
          if (clsValue > 0) {
            console.log(`📐 Cumulative Layout Shift: ${clsValue.toFixed(4)}`);
          }
        });
        clsObserver.observe({ entryTypes: ["layout-shift"] });
      } catch (error) {
        console.warn("Performance monitoring not fully supported:", error);
      }
    }
  }

  /**
   * Get current performance metrics
   */
  static getMetrics(): Record<string, number> {
    const navigation = performance.getEntriesByType(
      "navigation"
    )[0] as PerformanceNavigationTiming;

    if (!navigation) {
      return {};
    }

    return {
      domContentLoaded:
        navigation.domContentLoadedEventEnd -
        navigation.domContentLoadedEventStart,
      loadComplete: navigation.loadEventEnd - navigation.loadEventStart,
      firstByte: navigation.responseStart - navigation.requestStart,
      domInteractive: navigation.domInteractive - navigation.fetchStart,
      totalLoadTime: navigation.loadEventEnd - navigation.fetchStart,
    };
  }

  /**
   * Clear all timers and marks
   */
  static clear(): void {
    this.timers.clear();
    this.marks.clear();

    if (typeof performance.clearMarks === "function") {
      performance.clearMarks();
    }
    if (typeof performance.clearMeasures === "function") {
      performance.clearMeasures();
    }
  }
}

/**
 * React hook to monitor component render performance
 */
import { useEffect, useRef } from "react";

export const useRenderPerformance = (
  componentName: string,
  dependencies: any[] = []
) => {
  const renderCount = useRef(0);
  const lastRenderTime = useRef(performance.now());

  useEffect(() => {
    renderCount.current += 1;
    const currentTime = performance.now();
    const timeSinceLastRender = currentTime - lastRenderTime.current;

    console.log(
      `🔄 ${componentName} render #${
        renderCount.current
      } (${timeSinceLastRender.toFixed(2)}ms since last)`
    );

    lastRenderTime.current = currentTime;
  }, dependencies);

  useEffect(() => {
    PerformanceMonitor.mark(`${componentName}-mount`);

    return () => {
      PerformanceMonitor.mark(`${componentName}-unmount`);
    };
  }, [componentName]);
};

export default PerformanceMonitor;
