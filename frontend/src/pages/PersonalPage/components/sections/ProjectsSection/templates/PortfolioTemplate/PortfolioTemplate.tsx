import React from 'react';
import { Card, Typography, Row, Col, Tag } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import { getBackgroundWithPattern, getShadowStyle, getFontSize, getFontWeight, getBackgroundSize } from '../../../../../../../utils/templateUtils';
import './PortfolioTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface AdvancedSectionSettings {
  layout: any;
  background: any;
  typography: any;
  styling: any;
  spacing: any;
  animation: any;
}

interface PortfolioTemplateProps {
  projects: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
  advancedSettings?: AdvancedSectionSettings;
  editMode?: boolean;
}

const PortfolioTemplate: React.FC<PortfolioTemplateProps> = ({
  projects,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
  advancedSettings,
  editMode = true,
}) => {
  // Build card styles with advanced settings overrides
  const getCardStyles = () => {
    const defaultStyles = {
      marginBottom: '32px',
      borderRadius: '16px',
      position: 'relative' as const,
    };

    if (!advancedSettings) return defaultStyles;

    const { layout, background, styling } = advancedSettings;

    // Generate CSS custom properties for advanced settings
    const cssCustomProperties = {
      '--advanced-background': (background.color || background.pattern !== 'none') ? 
        getBackgroundWithPattern(background.color || '#ffffff', background.pattern, background.opacity) :
        undefined,
      '--advanced-background-size': getBackgroundSize(background.pattern),
      '--advanced-border-radius': styling.roundCorners ? styling.borderRadius : '0px',
      '--advanced-box-shadow': styling.shadow ? getShadowStyle(styling.shadowIntensity) : 'none',
      '--advanced-margin-bottom': `${layout.margin}${typeof layout.margin === 'number' ? 'px' : ''}`,
      '--advanced-margin-left': layout.fullscreen ? 'calc(-50vw + 50%)' : `${layout.margin}${typeof layout.margin === 'number' ? 'px' : ''}`,
      '--advanced-margin-right': layout.fullscreen ? 'calc(-50vw + 50%)' : `${layout.margin}${typeof layout.margin === 'number' ? 'px' : ''}`,
      '--advanced-border': styling.border.enabled 
        ? `${styling.border.width} ${styling.border.style} ${styling.border.color}`
        : 'none',
      '--advanced-overflow': 'hidden',
      '--advanced-width': layout.fullscreen ? '100vw' : 'auto',
    } as React.CSSProperties;

    // Add animation and transition as regular inline styles
    const finalStyles = {
      ...cssCustomProperties,
      position: 'relative' as const,
      transition: 'none', // Animation disabled
    };

    return finalStyles;
  };

  // Build content styles
  const getContentStyles = () => {
    if (!advancedSettings) return {};
    const { spacing } = advancedSettings;
    return {
      padding: '32px' // Default padding,
    };
  };

  // Build typography styles
  const getTypographyStyles = () => {
    if (!advancedSettings) return {};
    const { typography } = advancedSettings;
    return {
      fontSize: typography.fontSize ? getFontSize(typography.fontSize) : undefined,
      color: typography.fontColor || undefined,
      fontWeight: typography.fontWeight ? getFontWeight(typography.fontWeight) : undefined,
    };
  };

  const cardStyles = getCardStyles();
  const cardClassName = advancedSettings ? 'portfolio-template portfolio-template--custom' : 'portfolio-template';

  return (
    <Card 
      key="projects" 
      className={cardClassName}
      style={cardStyles}
    >
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
        editMode={editMode}
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
          Projects
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
                  {project.technologies && typeof project.technologies === 'string' && (
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