import React from 'react';
import { Card, Tag, Typography, Button } from 'antd';
import { UserOutlined, EnvironmentOutlined, LinkOutlined } from '@ant-design/icons';
import CachedAvatar from '@/components/common/CachedAvatar/CachedAvatar';
import { getProfilePictureUrl } from '@/utils/imageUtils';
import type { PublicPersonalPage } from '@/services/personalPageService';
import { useRouter } from '@/router/RouterProvider';
import './PortfolioCard.scss';

const { Title, Text, Paragraph } = Typography;

interface PortfolioCardProps {
  portfolio: PublicPersonalPage;
  onClick?: () => void;
}

export const PortfolioCard: React.FC<PortfolioCardProps> = ({ portfolio, onClick }) => {
  const { navigate } = useRouter();
  const { user, jobs, hobbies, projects, settings } = portfolio;

  // Get current job info
  const currentJob = jobs.find(job => !job.endDate) || jobs[0];
  
  // Get top technologies from projects
  const technologies = projects
    .flatMap(project => 
      typeof project.technologies === 'string' 
        ? project.technologies.split(',').map((tech: string) => tech.trim())
        : Array.isArray(project.technologies) 
        ? project.technologies 
        : []
    )
    .slice(0, 5); // Show max 5 technologies

  // Get top hobbies
  const topHobbies = hobbies.slice(0, 3);

  const handleCardClick = () => {
    if (onClick) {
      onClick();
    } else if (settings.pageSlug) {
      // Navigate to public portfolio page
      navigate('public', undefined, undefined, settings.pageSlug);
    }
  };

  return (
    <Card
      className="portfolio-card"
      hoverable
      onClick={handleCardClick}
      cover={
        <div className="portfolio-card__header">
          <CachedAvatar
            size={48}
            src={getProfilePictureUrl(user.avatar)}
            icon={<UserOutlined />}
            className="portfolio-card__avatar"
          />
        </div>
      }
      actions={[
        <Button 
          type="link" 
          icon={<LinkOutlined />} 
          onClick={(e) => {
            e.stopPropagation();
            handleCardClick();
          }}
        >
          View Portfolio
        </Button>
      ]}
    >
      <div className="portfolio-card__content">
        <Title level={4} className="portfolio-card__name">
          {user.name}
        </Title>
        
        {currentJob && (
          <div className="portfolio-card__job">
            <Text strong className="portfolio-card__position">
              {currentJob.position}
            </Text>
            {currentJob.company && (
              <Text className="portfolio-card__company">
                at {currentJob.company}
              </Text>
            )}
            {currentJob.location && (
              <div className="portfolio-card__location">
                <EnvironmentOutlined /> {currentJob.location}
              </div>
            )}
          </div>
        )}

        {user.bio && (
          <Paragraph 
            ellipsis={{ rows: 2 }} 
            className="portfolio-card__bio"
          >
            {user.bio}
          </Paragraph>
        )}

        {technologies.length > 0 && (
          <div className="portfolio-card__technologies">
            <Text className="portfolio-card__section-title">Technologies:</Text>
            <div className="portfolio-card__tags">
              {technologies.map((tech, index) => (
                <Tag key={index} className="portfolio-card__tech-tag">
                  {tech}
                </Tag>
              ))}
            </div>
          </div>
        )}

        {topHobbies.length > 0 && (
          <div className="portfolio-card__hobbies">
            <Text className="portfolio-card__section-title">Interests:</Text>
            <div className="portfolio-card__tags">
              {topHobbies.map((hobby) => (
                <Tag key={hobby.id} className="portfolio-card__hobby-tag">
                  {hobby.name}
                </Tag>
              ))}
            </div>
          </div>
        )}

        <div className="portfolio-card__stats">
          <div className="portfolio-card__stat">
            <Text strong>{projects.length}</Text>
            <Text> Project{projects.length !== 1 ? 's' : ''}</Text>
          </div>
          <div className="portfolio-card__stat">
            <Text strong>{jobs.length}</Text>
            <Text> Position{jobs.length !== 1 ? 's' : ''}</Text>
          </div>
          <div className="portfolio-card__stat">
            <Text strong>{hobbies.length}</Text>
            <Text> Interest{hobbies.length !== 1 ? 's' : ''}</Text>
          </div>
        </div>
      </div>
    </Card>
  );
};
