import React from 'react';
import { Card, Typography, Row, Col, Tag } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './PortfolioTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface PortfolioTemplateProps {
  projects: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const PortfolioTemplate: React.FC<PortfolioTemplateProps> = ({
  projects,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="projects" className="portfolio-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      {/* Portfolio Header */}
      <div 
        className="portfolio-template__header"
        style={{
          background: `linear-gradient(135deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}dd)`
        }}
      >
        <div className="portfolio-template__bg-pattern" />
        <Title level={2} className="portfolio-template__title">
          🖼️ Portfolio Gallery
        </Title>
        <Text className="portfolio-template__subtitle">
          Showcasing Creative Projects
        </Text>
      </div>
      
      <div className="portfolio-template__content">
        <Row gutter={[32, 32]}>
          {projects.map((project: any, index: number) => (
            <Col key={index} xs={24} md={12} lg={8}>
              <div className="portfolio-template__project-card">
                {/* Project Image Placeholder */}
                <div 
                  className="portfolio-template__project-image"
                  style={{
                    background: `linear-gradient(135deg, ${sectionPrimaryColor}20, ${sectionPrimaryColor}40)`
                  }}
                >
                  <div className="portfolio-template__project-icon">
                    {index === 0 ? '🎨' : index === 1 ? '💻' : index === 2 ? '📱' : '🚀'}
                  </div>
                  
                  {/* Project Badge */}
                  <div className="portfolio-template__project-badge">
                    <Text className="portfolio-template__badge-text" style={{ color: sectionPrimaryColor }}>
                      PROJECT #{index + 1}
                    </Text>
                  </div>
                </div>
                
                <div className="portfolio-template__project-content">
                  <Title level={4} className="portfolio-template__project-title" style={{ color: sectionPrimaryColor }}>
                    {project.name}
                  </Title>
                  
                  {project.description && (
                    <Paragraph className="portfolio-template__project-description">
                      {project.description.length > 100 ? project.description.substring(0, 100) + '...' : project.description}
                    </Paragraph>
                  )}
                  
                  {/* Tech Stack */}
                  {project.technologies && (
                    <div className="portfolio-template__tech-stack">
                      <Text className="portfolio-template__tech-label">
                        TECH STACK:
                      </Text>
                      <div className="portfolio-template__tech-tags">
                        {project.technologies.split(',').slice(0, 3).map((tech: string, techIndex: number) => (
                          <Tag 
                            key={techIndex} 
                            color={sectionPrimaryColor}
                            className="portfolio-template__tech-tag"
                          >
                            {tech.trim()}
                          </Tag>
                        ))}
                        {project.technologies.split(',').length > 3 && (
                          <Tag className="portfolio-template__tech-more">
                            +{project.technologies.split(',').length - 3}
                          </Tag>
                        )}
                      </div>
                    </div>
                  )}
                  
                  {/* Project Links */}
                  <div className="portfolio-template__project-links">
                    {project.startDate && (
                      <Text className="portfolio-template__project-date">
                        {project.startDate}
                      </Text>
                    )}
                    <div className="portfolio-template__links-container">
                      {project.demoUrl && (
                        <div 
                          className="portfolio-template__link-demo"
                          style={{
                            background: `${sectionPrimaryColor}15`
                          }}
                        >
                          <Text className="portfolio-template__link-text" style={{ color: sectionPrimaryColor }}>
                            🔗 DEMO
                          </Text>
                        </div>
                      )}
                      {project.sourceUrl && (
                        <div className="portfolio-template__link-code">
                          <Text className="portfolio-template__link-text">
                            💻 CODE
                          </Text>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            </Col>
          ))}
        </Row>
      </div>
    </Card>
  );
};

export default PortfolioTemplate; 