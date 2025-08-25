import React, { Suspense, ComponentType, LazyExoticComponent } from "react";
import PageLoader from "@/components/common/PageLoader/PageLoader";

/**
 * Higher-order component for lazy loading with custom fallback
 */
export const withLazyLoader = <P extends object>(
  LazyComponent: LazyExoticComponent<ComponentType<P>>,
  fallback?: React.ComponentType,
  errorMessage = "Failed to load page"
) => {
  const WrappedComponent = (props: P) => {
    const FallbackComponent =
      fallback || (() => <PageLoader message="Loading page..." />);

    return (
      <Suspense fallback={<FallbackComponent />}>
        <LazyComponent {...props} />
      </Suspense>
    );
  };

  WrappedComponent.displayName = `withLazyLoader(${
    LazyComponent.displayName || "Component"
  })`;

  return WrappedComponent;
};

/**
 * Utility function to create lazy-loaded pages with consistent loading states
 */
export const createLazyPage = (
  importFn: () => Promise<{ default: ComponentType<any> }>
) => {
  const LazyComponent = React.lazy(importFn);
  return withLazyLoader(LazyComponent);
};

export default { withLazyLoader, createLazyPage };
