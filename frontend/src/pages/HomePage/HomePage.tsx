import React, { useState } from "react";
import { Typography, Row, Col, Spin, Alert } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import { BlogCard } from "@common/BlogCard";
import { FloatingActionButton } from "@common/FloatingActionButton";
import Pagination from "@components/Pagination/Pagination";
import { useBlogData, usePaginatedPosts, useResponsive } from "@/hooks/useBlog";
import { useRouter } from "@/router/RouterProvider";
import { PAGINATION, BREAKPOINTS } from "@/utils/constants";
import "./HomePage.scss";

const { Title } = Typography;

const HomePage: React.FC = () => {
  const [currentPage, setCurrentPage] = useState(1);

  // Use custom hooks
  const {
    data: blogData,
    loading: blogLoading,
    error: blogError,
  } = useBlogData();
  const {
    data: currentPosts,
    pagination,
    loading: postsLoading,
    error: postsError,
  } = usePaginatedPosts(currentPage, PAGINATION.DEFAULT_PAGE_SIZE);
  const isMobile = useResponsive(BREAKPOINTS.MOBILE);
  const { navigate } = useRouter();

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    // Scroll to the top of the posts section
    document
      .querySelector(".home-page__grid-posts")
      ?.scrollIntoView({ behavior: "smooth" });
  };

  const handleCreateBlog = () => {
    navigate("create-blog");
  };

  // Show loading spinner while initial blog data is loading
  if (blogLoading) {
    return (
      <div className="home-page__loading">
        <Spin size="large" />
      </div>
    );
  }

  // Show error if blog data failed to load
  if (blogError) {
    return (
      <div className="home-page__error">
        <Alert
          message="Error Loading Content"
          description={blogError}
          type="error"
          showIcon
        />
      </div>
    );
  }

  const { featuredPosts, horizontalPosts } = blogData;

  return (
    <div className="home-page">
      {/* Hero Section */}
      <section className="home-page__section">
        <Title
          level={1}
          className={`home-page__title ${
            isMobile ? "home-page__title--mobile" : ""
          }`}
        >
          Today's Special
        </Title>

        {featuredPosts.length > 0 && (
          <div className="home-page__featured">
            <BlogCard post={featuredPosts[0]} variant="featured" />
          </div>
        )}
      </section>

      {/* Popular Recipes Section */}
      {horizontalPosts.length > 0 && (
        <section className="home-page__section">
          <Title
            level={2}
            className={`home-page__title ${
              isMobile ? "home-page__title--mobile" : ""
            }`}
          >
            Popular Recipes
          </Title>

          <div className="home-page__horizontal-list">
            {horizontalPosts.map((post) => (
              <BlogCard key={post.id} post={post} variant="horizontal" />
            ))}
          </div>
        </section>
      )}

      {/* All Recipes Section */}
      <section className="home-page__section">
        <Title
          level={2}
          className={`home-page__title ${
            isMobile ? "home-page__title--mobile" : ""
          }`}
        >
          All Recipes
        </Title>

        {postsError ? (
          <Alert
            message="Error Loading Posts"
            description={postsError}
            type="error"
            showIcon
          />
        ) : (
          <>
            <div className="home-page__grid-posts">
              {postsLoading ? (
                <div style={{ textAlign: "center", padding: "50px" }}>
                  <Spin size="large" />
                </div>
              ) : (
                <Row gutter={[12, 24]}>
                  {currentPosts.map((post) => (
                    <Col key={post.id} xs={24} sm={24} md={12} lg={8} xl={8}>
                      <BlogCard post={post} />
                    </Col>
                  ))}
                </Row>
              )}
            </div>

            {pagination.totalPages > 1 && (
              <div className="home-page__pagination">
                <Pagination
                  currentPage={currentPage}
                  totalPages={pagination.totalPages}
                  onPageChange={handlePageChange}
                />
              </div>
            )}
          </>
        )}
      </section>

      {/* Floating Action Button for creating new blog */}
      <FloatingActionButton
        onClick={handleCreateBlog}
        icon={<PlusOutlined />}
      />
    </div>
  );
};

export default HomePage;
