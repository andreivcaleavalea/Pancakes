import React, { useEffect, useState } from "react";
import {
  Typography,
  Spin,
  Alert,
  Empty,
  Row,
  Col,
  Button,
  Space,
  Card,
  Tooltip,
  Popconfirm,
  message,
} from "antd";
import {
  EditOutlined,
  FileTextOutlined,
  SendOutlined,
  DeleteOutlined,
  EyeOutlined,
  ReloadOutlined,
} from "@ant-design/icons";
import { useAuth } from "@/contexts/AuthContext";
import { useRouter } from "@/router/RouterProvider";
import { blogPostsApi } from "@/services/blogApi";
import type { BlogPost, PaginatedResult } from "@/types/blog";
import { getImageUrl } from "@/utils/imageUtils";
import { useDrafts } from "@/hooks/useBlog";
import "./DraftsPage.scss";

const { Title, Text, Paragraph } = Typography;

const DraftsPage: React.FC = () => {
  const { isAuthenticated } = useAuth();
  const { navigate, currentPage } = useRouter();
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const [currentPageNum, setCurrentPageNum] = useState(1);
  const [pageSize] = useState(10);

  // Use the new hook for data fetching
  const {
    data: drafts,
    pagination,
    loading,
    error,
    refetch,
  } = useDrafts(currentPageNum, pageSize);

  // Redirect to login if not authenticated
  useEffect(() => {
    if (!isAuthenticated) {
      navigate("login");
    }
  }, [isAuthenticated, navigate]);

  // Reload drafts whenever we navigate to the drafts page (with debouncing)
  useEffect(() => {
    if (isAuthenticated && currentPage === "drafts") {
      console.log("🔄 [DraftsPage] Page navigation detected, calling refetch");
      // Add a small delay to allow any pending cache clears to complete
      const timeoutId = setTimeout(() => {
        refetch();
      }, 100);

      return () => clearTimeout(timeoutId);
    }
  }, [currentPage, isAuthenticated, refetch]);

  // Debug: Log when drafts data changes
  useEffect(() => {
    console.log("📊 [DraftsPage] Drafts data updated:", {
      count: drafts.length,
      firstDraftTitle: drafts[0]?.title,
      firstDraftUpdatedAt: drafts[0]?.updatedAt,
      loading,
    });
  }, [drafts, loading]);

  // Listen for page visibility changes to refresh data when tab becomes visible (with debouncing)
  useEffect(() => {
    let visibilityTimeoutId: NodeJS.Timeout;

    const handleVisibilityChange = () => {
      if (
        document.visibilityState === "visible" &&
        isAuthenticated &&
        currentPage === "drafts"
      ) {
        console.log("Page became visible, refreshing drafts");
        // Debounce visibility changes to prevent rapid-fire refreshes
        clearTimeout(visibilityTimeoutId);
        visibilityTimeoutId = setTimeout(() => {
          refetch();
        }, 500);
      }
    };

    document.addEventListener("visibilitychange", handleVisibilityChange);
    return () => {
      document.removeEventListener("visibilitychange", handleVisibilityChange);
      clearTimeout(visibilityTimeoutId);
    };
  }, [isAuthenticated, currentPage, refetch]);

  const handlePublishDraft = async (draftId: string) => {
    try {
      setActionLoading(draftId);
      await blogPostsApi.publishDraft(draftId);
      message.success("Draft published successfully!");
      console.log("🚀 [DraftsPage] Draft published, calling refetch");
      // Refresh the drafts list
      refetch();
    } catch (err) {
      message.error(
        err instanceof Error ? err.message : "Failed to publish draft"
      );
    } finally {
      setActionLoading(null);
    }
  };

  const handleDeleteDraft = async (draftId: string) => {
    try {
      setActionLoading(draftId);
      console.log("Deleting draft with ID:", draftId);

      await blogPostsApi.delete(draftId);
      console.log("Delete API call completed successfully");

      message.success("Draft deleted successfully!");
      console.log("🗑️ [DraftsPage] Draft deleted, calling refetch");
      // Refresh the drafts list
      refetch();
    } catch (err) {
      console.error("Delete failed:", err);
      message.error(
        err instanceof Error ? err.message : "Failed to delete draft"
      );
    } finally {
      setActionLoading(null);
    }
  };

  const handleEditDraft = (draftId: string) => {
    navigate("edit-blog", undefined, draftId);
  };

  const handlePreviewDraft = (draftId: string) => {
    navigate("blog-view", undefined, draftId);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  if (!isAuthenticated) {
    return null; // Will redirect to login
  }

  if (loading) {
    return (
      <div className="drafts-page">
        <div className="drafts-page__header">
          <Title level={2} className="drafts-page__title">
            <FileTextOutlined /> My Drafts
          </Title>
        </div>
        <div className="drafts-page__loading">
          <Spin size="large" />
          <Text>Loading your drafts...</Text>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="drafts-page">
        <div className="drafts-page__header">
          <Title level={2} className="drafts-page__title">
            <FileTextOutlined /> My Drafts
          </Title>
        </div>
        <Alert
          message="Error Loading Drafts"
          description={error}
          type="error"
          showIcon
          action={
            <Button size="small" onClick={refetch}>
              Retry
            </Button>
          }
        />
      </div>
    );
  }

  if (drafts.length === 0) {
    return (
      <div className="drafts-page">
        <div className="drafts-page__header">
          <Title level={2} className="drafts-page__title">
            <FileTextOutlined /> My Drafts
          </Title>
        </div>
        <Empty
          description="No drafts found"
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          className="drafts-page__empty"
        >
          <Button type="primary" onClick={() => navigate("create-blog")}>
            Create Your First Draft
          </Button>
        </Empty>
      </div>
    );
  }

  return (
    <div className="drafts-page">
      <div className="drafts-page__header">
        <div className="drafts-page__title-section">
          <Title level={2} className="drafts-page__title">
            <FileTextOutlined /> My Drafts
          </Title>
          <Space>
            <Button
              icon={<ReloadOutlined />}
              onClick={() => {
                console.log("🔃 [DraftsPage] Manual refresh button clicked");
                refetch();
              }}
              loading={loading}
            >
              Refresh
            </Button>
            <Button type="primary" onClick={() => navigate("create-blog")}>
              Create New Draft
            </Button>
          </Space>
        </div>
        <Text type="secondary" className="drafts-page__subtitle">
          {pagination.totalItems} draft{pagination.totalItems !== 1 ? "s" : ""}{" "}
          found
        </Text>
      </div>

      <div className="drafts-page__content">
        <Row gutter={[16, 16]}>
          {drafts.map((draft) => (
            <Col key={draft.id} xs={24} md={12} lg={8}>
              <Card
                className="drafts-page__draft-card"
                actions={[
                  <Tooltip title="Edit Draft">
                    <Button
                      type="text"
                      icon={<EditOutlined />}
                      onClick={() => handleEditDraft(draft.id)}
                      loading={actionLoading === draft.id}
                    />
                  </Tooltip>,
                  <Tooltip title="Preview">
                    <Button
                      type="text"
                      icon={<EyeOutlined />}
                      onClick={() => handlePreviewDraft(draft.id)}
                    />
                  </Tooltip>,
                  <Tooltip title="Publish">
                    <Popconfirm
                      title="Publish this draft?"
                      description="This will make your blog post visible to all users."
                      onConfirm={() => handlePublishDraft(draft.id)}
                      okText="Publish"
                      cancelText="Cancel"
                    >
                      <Button
                        type="text"
                        icon={<SendOutlined />}
                        loading={actionLoading === draft.id}
                      />
                    </Popconfirm>
                  </Tooltip>,
                  <Tooltip title="Delete">
                    <Popconfirm
                      title="Delete this draft?"
                      description="This action cannot be undone."
                      onConfirm={() => handleDeleteDraft(draft.id)}
                      okText="Delete"
                      cancelText="Cancel"
                    >
                      <Button
                        type="text"
                        icon={<DeleteOutlined />}
                        danger
                        loading={actionLoading === draft.id}
                      />
                    </Popconfirm>
                  </Tooltip>,
                ]}
              >
                <div className="drafts-page__draft-content">
                  {draft.featuredImage && (
                    <div className="drafts-page__draft-image">
                      <img
                        src={getImageUrl(draft.featuredImage)}
                        alt={draft.title}
                        className="drafts-page__image"
                        onError={(e) => {
                          // Hide image on error
                          const target = e.target as HTMLImageElement;
                          target.style.display = "none";
                        }}
                      />
                    </div>
                  )}

                  <Title
                    level={4}
                    className="drafts-page__draft-title"
                    ellipsis={{ rows: 2 }}
                  >
                    {draft.title}
                  </Title>

                  <Paragraph
                    className="drafts-page__draft-excerpt"
                    ellipsis={{ rows: 3 }}
                    type="secondary"
                  >
                    {draft.content.replace(/<[^>]*>/g, "").substring(0, 150)}...
                  </Paragraph>

                  <div className="drafts-page__draft-meta">
                    <Space direction="vertical" size="small">
                      <Text
                        type="secondary"
                        className="drafts-page__draft-date"
                      >
                        Created: {formatDate(draft.createdAt)}
                      </Text>
                      <Text
                        type="secondary"
                        className="drafts-page__draft-date"
                      >
                        Updated: {formatDate(draft.updatedAt)}
                      </Text>
                      {draft.tags &&
                        Array.isArray(draft.tags) &&
                        draft.tags.length > 0 && (
                          <div className="drafts-page__draft-tags">
                            {draft.tags.slice(0, 3).map((tag, index) => (
                              <span key={index} className="drafts-page__tag">
                                #{tag}
                              </span>
                            ))}
                            {draft.tags.length > 3 && (
                              <span className="drafts-page__tag-more">
                                +{draft.tags.length - 3} more
                              </span>
                            )}
                          </div>
                        )}
                    </Space>
                  </div>
                </div>
              </Card>
            </Col>
          ))}
        </Row>
      </div>
    </div>
  );
};

export default DraftsPage;
