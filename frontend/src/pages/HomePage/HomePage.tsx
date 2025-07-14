import React, { useState, useEffect } from "react";
import { Typography, Row, Col } from "antd";
import { BlogCard } from "@common/BlogCard";
import Pagination from "@components/Pagination/Pagination";
import { blogData } from "@utils/mockData";
import "./HomePage.scss";

const { Title } = Typography;

const HomePage: React.FC = () => {
  const { featuredPosts, gridPosts, horizontalPosts } = blogData;
  const [isMobile, setIsMobile] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const postsPerPage = 9;

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= 768);
    };

    window.addEventListener("resize", handleResize);
    handleResize();

    return () => window.removeEventListener("resize", handleResize);
  }, []);

  // Calculate paginated posts
  const indexOfLastPost = currentPage * postsPerPage;
  const indexOfFirstPost = indexOfLastPost - postsPerPage;
  const currentPosts = gridPosts.slice(indexOfFirstPost, indexOfLastPost);
  const totalPages = Math.ceil(gridPosts.length / postsPerPage);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    // Scroll to the top of the posts section
    document
      .querySelector(".home-page__grid-posts")
      ?.scrollIntoView({ behavior: "smooth" });
  };

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
          Your pancakes
        </Title>

        {featuredPosts.length > 0 && (
          <div className="home-page__featured">
            <BlogCard post={featuredPosts[0]} variant="featured" />
          </div>
        )}
      </section>

      {/* Featured Posts Section */}
      {horizontalPosts.length > 0 && (
        <section className="home-page__section">
          <Title
            level={2}
            className={`home-page__title ${
              isMobile ? "home-page__title--mobile" : ""
            }`}
          >
            Featured posts
          </Title>

          <div className="home-page__horizontal-list">
            {horizontalPosts.map((post) => (
              <BlogCard key={post.id} post={post} variant="horizontal" />
            ))}
          </div>
        </section>
      )}

      {/* All Blog Posts Section */}
      <section className="home-page__section">
        <Title
          level={2}
          className={`home-page__title ${
            isMobile ? "home-page__title--mobile" : ""
          }`}
        >
          All blog posts
        </Title>

        <div className="home-page__grid-posts">
          <Row gutter={[12, 24]}>
            {currentPosts.map((post) => (
              <Col key={post.id} xs={24} sm={24} md={12} lg={8} xl={8}>
                <BlogCard post={post} />
              </Col>
            ))}
          </Row>
        </div>

        <div className="home-page__pagination">
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            onPageChange={handlePageChange}
          />
        </div>
      </section>
    </div>
  );
};

export default HomePage;
