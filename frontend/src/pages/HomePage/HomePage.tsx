import React, { useState, useEffect } from 'react';
import { Typography, Row, Col } from 'antd';
import { BlogCard } from '@common/BlogCard';
import Pagination from '@components/Pagination/Pagination';
import { fetchBlogData, fetchPaginatedPosts } from './api';
import type { BlogPost, FeaturedPost } from '@/types/blog';
import './HomePage.scss';

const { Title } = Typography;

const HomePage: React.FC = () => {
  const [blogData, setBlogData] = useState<{
    featuredPosts: FeaturedPost[];
    horizontalPosts: BlogPost[];
    gridPosts: BlogPost[];
  }>({
    featuredPosts: [],
    horizontalPosts: [],
    gridPosts: []
  });
  const [isMobile, setIsMobile] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [currentPosts, setCurrentPosts] = useState<BlogPost[]>([]);
  const [totalPages, setTotalPages] = useState(0);
  const postsPerPage = 9;

  useEffect(() => {
    const loadBlogData = async () => {
      const data = await fetchBlogData();
      setBlogData(data);
    };
    
    loadBlogData();
  }, []);

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= 768);
    };

    window.addEventListener('resize', handleResize);
    handleResize();
    
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  useEffect(() => {
    const loadPaginatedPosts = async () => {
      const { posts, totalPages: pages } = await fetchPaginatedPosts(currentPage, postsPerPage);
      setCurrentPosts(posts);
      setTotalPages(pages);
    };
    
    loadPaginatedPosts();
  }, [currentPage]);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    // Scroll to the top of the posts section
    document.querySelector('.home-page__grid-posts')?.scrollIntoView({ behavior: 'smooth' });
  };

  const { featuredPosts, horizontalPosts } = blogData;

  return (
    <div className="home-page">
      {/* Hero Section */}
      <section className="home-page__section">
        <Title level={1} className={`home-page__title ${isMobile ? 'home-page__title--mobile' : ''}`}>
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
          <Title level={2} className={`home-page__title ${isMobile ? 'home-page__title--mobile' : ''}`}>
            Popular Recipes
          </Title>
          
          <div className="home-page__horizontal-list">
            {horizontalPosts.map(post => (
              <BlogCard key={post.id} post={post} variant="horizontal" />
            ))}
          </div>
        </section>
      )}

      {/* All Recipes Section */}
      <section className="home-page__section">
        <Title level={2} className={`home-page__title ${isMobile ? 'home-page__title--mobile' : ''}`}>
          All Recipes
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