import React, { useMemo } from "react";
import { Row, Col, Empty } from "antd";
import { BlogCard, BlogCardSkeleton } from "@/components/common";
import type { BlogPost } from "@/types/blog";

interface BlogGridProps {
  posts: BlogPost[];
  loading?: boolean;
  averageRatings?: Record<
    string,
    { averageRating: number; totalRatings: number }
  >;
  variant?: "default" | "horizontal" | "featured";
  columns?: {
    xs?: number;
    sm?: number;
    md?: number;
    lg?: number;
    xl?: number;
  };
  gutter?: [number, number];
  emptyMessage?: string;
  skeletonCount?: number;
}

const BlogGrid: React.FC<BlogGridProps> = ({
  posts,
  loading = false,
  averageRatings = {},
  variant = "default",
  columns = { xs: 24, sm: 24, md: 12, lg: 8, xl: 8 },
  gutter = [12, 24],
  emptyMessage = "No blog posts found",
  skeletonCount = 6,
}) => {
  // Memoize skeleton items to prevent unnecessary re-renders
  const skeletonItems = useMemo(() => {
    return Array.from({ length: skeletonCount }, (_, index) => (
      <Col key={`skeleton-${index}`} {...columns}>
        <BlogCardSkeleton variant={variant} />
      </Col>
    ));
  }, [skeletonCount, columns, variant]);

  // Memoize blog card items
  const blogItems = useMemo(() => {
    return posts.map((post) => (
      <Col key={post.id} {...columns}>
        <BlogCard
          post={post}
          variant={variant}
          averageRating={averageRatings[post.id]}
        />
      </Col>
    ));
  }, [posts, columns, variant, averageRatings]);

  if (loading) {
    return <Row gutter={gutter}>{skeletonItems}</Row>;
  }

  if (posts.length === 0) {
    return (
      <Empty
        image={Empty.PRESENTED_IMAGE_SIMPLE}
        description={emptyMessage}
        style={{ margin: "40px 0" }}
      />
    );
  }

  return <Row gutter={gutter}>{blogItems}</Row>;
};

export default BlogGrid;
