import React from "react";
import { Card, Skeleton, Space } from "antd";

interface BlogCardSkeletonProps {
  variant?: "default" | "horizontal" | "featured";
}

const BlogCardSkeleton: React.FC<BlogCardSkeletonProps> = ({
  variant = "default",
}) => {
  if (variant === "horizontal") {
    return (
      <div className="blog-card blog-card--horizontal">
        <Skeleton.Image style={{ width: 120, height: 80 }} active />
        <div
          className="blog-card__content"
          style={{ flex: 1, padding: "0 16px" }}
        >
          <Space direction="vertical" size="small" style={{ width: "100%" }}>
            <Skeleton.Input style={{ width: 80, height: 14 }} active />
            <Skeleton.Input style={{ width: "90%", height: 20 }} active />
            <Skeleton.Input style={{ width: "100%", height: 16 }} active />
            <Skeleton.Input style={{ width: "100%", height: 16 }} active />
            <Space>
              <Skeleton.Avatar size={32} active />
              <Skeleton.Input style={{ width: 100, height: 16 }} active />
            </Space>
          </Space>
        </div>
      </div>
    );
  }

  return (
    <Card
      className={`blog-card ${
        variant === "featured" ? "blog-card--featured" : ""
      }`}
      variant="borderless"
    >
      <Skeleton.Image
        style={{
          width: "100%",
          height: variant === "featured" ? 300 : 200,
        }}
        active
      />
      <div style={{ padding: "16px 0" }}>
        <Space direction="vertical" size="small" style={{ width: "100%" }}>
          <Skeleton.Input style={{ width: 100, height: 14 }} active />
          <Skeleton.Input
            style={{
              width: "90%",
              height: variant === "featured" ? 28 : 20,
            }}
            active
          />
          <Space>
            <Skeleton.Avatar size={variant === "featured" ? 40 : 32} active />
            <Skeleton.Input style={{ width: 120, height: 16 }} active />
          </Space>
          <Skeleton.Input style={{ width: "100%", height: 16 }} active />
          <Skeleton.Input style={{ width: "80%", height: 16 }} active />
        </Space>
      </div>
    </Card>
  );
};

export default BlogCardSkeleton;
