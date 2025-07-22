import React from 'react';
import { Card, Typography, Row, Col, Tag, Button } from 'antd';
import { LinkOutlined, GithubOutlined } from '@ant-design/icons';
import SectionSettingsPopover from '../SectionSettingsPopover';
import type { SectionRendererProps } from '../../types';
import { SECTION_COLORS } from '../../constants';

const { Title, Text, Paragraph } = Typography;

interface ProjectsSectionProps extends SectionRendererProps {
  projects?: any[];
}

const ProjectsSection: React.FC<ProjectsSectionProps> = ({
  sectionKey,
  user,
  data,
  primaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
  projects,
}) => {
  // Use either data prop or projects prop
  const projectsData = projects || data || [];
  const { template } = currentSectionSettings;
  const sectionPrimaryColor = SECTION_COLORS[currentSectionSettings.color as keyof typeof SECTION_COLORS] || SECTION_COLORS.blue;

  const renderGridTemplate = () => (
    <Card key="projects" style={{ 
      marginBottom: '32px', 
      borderRadius: '16px',
      position: 'relative'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor, marginBottom: '32px' }}>
        🎨 Creative Grid
      </Title>
      
      <Row gutter={[24, 24]}>
        {projectsData.map((project: any, index: number) => (
          <Col key={index} xs={24} sm={12} md={8}>
            <Card
              hoverable
              style={{
                height: '100%',
                borderRadius: '16px',
                border: `2px solid ${sectionPrimaryColor}15`,
                overflow: 'hidden',
                position: 'relative'
              }}
              bodyStyle={{ padding: '20px' }}
            >
              <div style={{
                position: 'absolute',
                top: 0,
                left: 0,
                width: '100%',
                height: '4px',
                background: `linear-gradient(90deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`
              }} />
              
              <div style={{ textAlign: 'center', marginBottom: '16px' }}>
                <div style={{
                  background: `${sectionPrimaryColor}15`,
                  borderRadius: '50%',
                  width: '60px',
                  height: '60px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  margin: '0 auto 12px',
                  fontSize: '24px'
                }}>
                  🚀
                </div>
              </div>
              
              <Title level={4} style={{ 
                textAlign: 'center',
                color: sectionPrimaryColor,
                marginBottom: '12px'
              }}>
                {project.name}
              </Title>
              
              <Paragraph style={{ 
                fontSize: '13px', 
                color: '#666',
                lineHeight: 1.5,
                textAlign: 'center',
                minHeight: '60px'
              }}>
                {project.description}
              </Paragraph>
              
              {project.technologies && (
                <div style={{ marginBottom: '16px', textAlign: 'center' }}>
                  {project.technologies.split(',').map((tech: string, techIndex: number) => (
                    <Tag key={techIndex} style={{ marginBottom: '4px' }}>
                      {tech.trim()}
                    </Tag>
                  ))}
                </div>
              )}
              
              <div style={{ display: 'flex', justifyContent: 'center', gap: '8px' }}>
                {project.githubUrl && (
                  <Button 
                    size="small" 
                    icon={<GithubOutlined />}
                    href={project.githubUrl}
                    target="_blank"
                  >
                    Code
                  </Button>
                )}
                {project.liveUrl && (
                  <Button 
                    size="small" 
                    type="primary"
                    icon={<LinkOutlined />}
                    href={project.liveUrl}
                    target="_blank"
                  >
                    Live
                  </Button>
                )}
              </div>
            </Card>
          </Col>
        ))}
      </Row>
    </Card>
  );

  const renderShowcaseTemplate = () => (
    <Card key="projects" style={{ 
      marginBottom: '32px', 
      borderRadius: '16px',
      position: 'relative'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor, textAlign: 'center', marginBottom: '32px' }}>
        🚀 Project Showcase
      </Title>
      
      {projectsData.map((project: any, index: number) => (
        <Card key={index} style={{ 
          marginBottom: '24px',
          border: `1px solid ${sectionPrimaryColor}20`,
          borderRadius: '12px'
        }}>
          <Row align="middle" gutter={[24, 16]}>
            <Col xs={24} sm={16}>
              <Title level={3} style={{ color: sectionPrimaryColor, marginBottom: '8px' }}>
                {project.name}
              </Title>
              <Paragraph style={{ marginBottom: '12px', color: '#666' }}>
                {project.description}
              </Paragraph>
              {project.technologies && (
                <div style={{ marginBottom: '12px' }}>
                  <Text strong style={{ fontSize: '12px', marginRight: '8px' }}>Tech Stack:</Text>
                  {project.technologies.split(',').map((tech: string, techIndex: number) => (
                    <Tag key={techIndex} color={sectionPrimaryColor}>
                      {tech.trim()}
                    </Tag>
                  ))}
                </div>
              )}
            </Col>
            <Col xs={24} sm={8} style={{ textAlign: 'center' }}>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                {project.githubUrl && (
                  <Button 
                    icon={<GithubOutlined />}
                    href={project.githubUrl}
                    target="_blank"
                    block
                  >
                    View Code
                  </Button>
                )}
                {project.liveUrl && (
                  <Button 
                    type="primary"
                    icon={<LinkOutlined />}
                    href={project.liveUrl}
                    target="_blank"
                    block
                  >
                    Live Demo
                  </Button>
                )}
              </div>
            </Col>
          </Row>
        </Card>
      ))}
    </Card>
  );

  const renderMinimalTemplate = () => (
    <Card key="projects" style={{ marginBottom: '24px', position: 'relative' }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor }}>🚀 Projects</Title>
      
      {projectsData.map((project: any, index: number) => (
        <div key={index} style={{ 
          padding: '20px 0', 
          borderBottom: index < projectsData.length - 1 ? `1px solid ${sectionPrimaryColor}20` : 'none' 
        }}>
          <Title level={4} style={{ margin: '0 0 12px', color: sectionPrimaryColor }}>
            {project.name}
          </Title>
          <Paragraph style={{ marginBottom: '12px' }}>
            {project.description}
          </Paragraph>
          {project.technologies && (
            <div style={{ marginBottom: '12px' }}>
              <Text style={{ fontSize: '12px', color: '#666', marginBottom: '8px', display: 'block' }}>
                Technologies:
              </Text>
              {project.technologies.split(',').map((tech: string, techIndex: number) => (
                <Tag key={techIndex}>
                  {tech.trim()}
                </Tag>
              ))}
            </div>
          )}
          <div style={{ display: 'flex', gap: '8px' }}>
            {project.githubUrl && (
              <Button size="small" icon={<GithubOutlined />} href={project.githubUrl} target="_blank">
                Code
              </Button>
            )}
            {project.liveUrl && (
              <Button size="small" type="primary" icon={<LinkOutlined />} href={project.liveUrl} target="_blank">
                Demo
              </Button>
            )}
          </div>
        </div>
      ))}
    </Card>
  );

  const renderPortfolioTemplate = () => (
    <Card key="projects" style={{ 
      marginBottom: '32px', 
      borderRadius: '20px',
      position: 'relative',
      background: 'linear-gradient(135deg, #f8f9fa 0%, #ffffff 100%)',
      border: 'none',
      boxShadow: '0 20px 40px rgba(0,0,0,0.1)'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      {/* Portfolio Header */}
      <div style={{
        background: `linear-gradient(135deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}dd)`,
        padding: '32px',
        textAlign: 'center',
        borderRadius: '20px 20px 0 0',
        position: 'relative'
      }}>
        <Title level={2} style={{ color: 'white', margin: 0, fontSize: '28px', fontWeight: '700' }}>
          🖼️ Portfolio Gallery
        </Title>
        <Text style={{ color: 'rgba(255,255,255,0.9)', fontSize: '16px', marginTop: '8px' }}>
          Showcase of Creative Work
        </Text>
      </div>
      
      <div style={{ padding: '32px' }}>
        <Row gutter={[24, 24]}>
          {projectsData.map((project: any, index: number) => (
            <Col key={index} xs={24} sm={12} md={8}>
              <div style={{
                background: '#fff',
                borderRadius: '16px',
                overflow: 'hidden',
                border: `1px solid ${sectionPrimaryColor}15`,
                position: 'relative',
                boxShadow: '0 8px 25px rgba(0,0,0,0.08)',
                transition: 'transform 0.3s ease, box-shadow 0.3s ease',
                cursor: 'pointer'
              }}>
                {/* Project Image Placeholder */}
                <div style={{
                  height: '180px',
                  background: `linear-gradient(135deg, ${sectionPrimaryColor}20, ${sectionPrimaryColor}40)`,
                  position: 'relative',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '48px'
                }}>
                  🚀
                  {/* Portfolio Badge */}
                  <div style={{
                    position: 'absolute',
                    top: '12px',
                    right: '12px',
                    background: 'rgba(255,255,255,0.9)',
                    color: sectionPrimaryColor,
                    padding: '4px 8px',
                    borderRadius: '8px',
                    fontSize: '10px',
                    fontWeight: '600'
                  }}>
                    #{index + 1}
                  </div>
                </div>
                
                <div style={{ padding: '20px' }}>
                  <Title level={4} style={{ 
                    color: sectionPrimaryColor,
                    marginBottom: '8px',
                    fontSize: '16px'
                  }}>
                    {project.name}
                  </Title>
                  
                  <Paragraph style={{ 
                    fontSize: '13px', 
                    color: '#666',
                    lineHeight: 1.5,
                    minHeight: '40px',
                    marginBottom: '12px'
                  }}>
                    {project.description.length > 60 ? project.description.substring(0, 60) + '...' : project.description}
                  </Paragraph>
                  
                  {project.technologies && (
                    <div style={{ marginBottom: '16px' }}>
                      {project.technologies.split(',').slice(0, 3).map((tech: string, techIndex: number) => (
                        <Tag key={techIndex} style={{ 
                          marginBottom: '4px',
                          fontSize: '10px',
                          padding: '2px 6px'
                        }}>
                          {tech.trim()}
                        </Tag>
                      ))}
                      {project.technologies.split(',').length > 3 && (
                        <Text style={{ fontSize: '10px', color: '#999' }}>
                          +{project.technologies.split(',').length - 3} more
                        </Text>
                      )}
                    </div>
                  )}
                  
                  <div style={{ 
                    display: 'flex', 
                    justifyContent: 'space-between',
                    alignItems: 'center'
                  }}>
                    <div style={{ display: 'flex', gap: '6px' }}>
                      {project.githubUrl && (
                        <Button 
                          size="small" 
                          icon={<GithubOutlined />}
                          href={project.githubUrl}
                          target="_blank"
                          style={{ fontSize: '10px', padding: '2px 8px', height: 'auto' }}
                        />
                      )}
                      {project.liveUrl && (
                        <Button 
                          size="small" 
                          type="primary"
                          icon={<LinkOutlined />}
                          href={project.liveUrl}
                          target="_blank"
                          style={{ fontSize: '10px', padding: '2px 8px', height: 'auto' }}
                        />
                      )}
                    </div>
                    <Text style={{ fontSize: '10px', color: sectionPrimaryColor, fontWeight: '600' }}>
                      VIEW DETAILS
                    </Text>
                  </div>
                </div>
              </div>
            </Col>
          ))}
        </Row>
      </div>
    </Card>
  );

  const renderCardsTemplate = () => (
    <Card key="projects" style={{ 
      marginBottom: '32px', 
      borderRadius: '16px',
      position: 'relative'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor, textAlign: 'center', marginBottom: '32px' }}>
        🃏 Project Cards
      </Title>
      
      <Row gutter={[24, 24]}>
        {projectsData.map((project: any, index: number) => (
          <Col key={index} xs={24} sm={12}>
            <div style={{
              background: 'linear-gradient(135deg, #fff 0%, #f8f9fa 100%)',
              borderRadius: '20px',
              padding: '28px',
              border: `2px solid ${sectionPrimaryColor}15`,
              position: 'relative',
              overflow: 'hidden',
              boxShadow: '0 12px 30px rgba(0,0,0,0.1)',
              minHeight: '280px'
            }}>
              {/* Card Number */}
              <div style={{
                position: 'absolute',
                top: '-10px',
                right: '-10px',
                width: '50px',
                height: '50px',
                background: `linear-gradient(135deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}80)`,
                borderRadius: '50%',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'white',
                fontSize: '14px',
                fontWeight: '700',
                boxShadow: '0 4px 12px rgba(0,0,0,0.15)'
              }}>
                {index + 1}
              </div>
              
              {/* Card Suit Symbol */}
              <div style={{
                position: 'absolute',
                top: '16px',
                left: '16px',
                fontSize: '20px',
                opacity: 0.3
              }}>
                {index % 4 === 0 ? '♠️' : index % 4 === 1 ? '♥️' : index % 4 === 2 ? '♦️' : '♣️'}
              </div>
              
              <div style={{ textAlign: 'center', marginBottom: '20px', marginTop: '20px' }}>
                <div style={{
                  background: `linear-gradient(135deg, ${sectionPrimaryColor}15, ${sectionPrimaryColor}25)`,
                  borderRadius: '16px',
                  width: '80px',
                  height: '80px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  margin: '0 auto 16px',
                  fontSize: '32px'
                }}>
                  🚀
                </div>
              </div>
              
              <Title level={3} style={{ 
                textAlign: 'center',
                color: sectionPrimaryColor,
                marginBottom: '12px',
                fontSize: '18px'
              }}>
                {project.name}
              </Title>
              
              <Paragraph style={{ 
                fontSize: '14px', 
                color: '#666',
                lineHeight: 1.6,
                textAlign: 'center',
                marginBottom: '16px',
                minHeight: '60px'
              }}>
                {project.description}
              </Paragraph>
              
              {project.technologies && (
                <div style={{ 
                  textAlign: 'center',
                  marginBottom: '20px'
                }}>
                  <Text style={{ fontSize: '11px', color: '#999', display: 'block', marginBottom: '8px' }}>
                    TECH STACK
                  </Text>
                  <div style={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'center', gap: '4px' }}>
                    {project.technologies.split(',').slice(0, 4).map((tech: string, techIndex: number) => (
                      <span key={techIndex} style={{
                        background: `${sectionPrimaryColor}10`,
                        color: sectionPrimaryColor,
                        padding: '2px 6px',
                        borderRadius: '4px',
                        fontSize: '9px',
                        fontWeight: '600'
                      }}>
                        {tech.trim()}
                      </span>
                    ))}
                  </div>
                </div>
              )}
              
              <div style={{ 
                display: 'flex', 
                justifyContent: 'center',
                gap: '8px',
                marginTop: 'auto'
              }}>
                {project.githubUrl && (
                  <Button 
                    size="small" 
                    icon={<GithubOutlined />}
                    href={project.githubUrl}
                    target="_blank"
                    style={{ borderRadius: '8px' }}
                  />
                )}
                {project.liveUrl && (
                  <Button 
                    size="small" 
                    type="primary"
                    icon={<LinkOutlined />}
                    href={project.liveUrl}
                    target="_blank"
                    style={{ borderRadius: '8px' }}
                  />
                )}
              </div>
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  );

  const renderDetailedTemplate = () => (
    <Card key="projects" style={{ 
      marginBottom: '32px', 
      borderRadius: '16px',
      position: 'relative'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor, marginBottom: '32px' }}>
        📋 Detailed View
      </Title>
      
      {projectsData.map((project: any, index: number) => (
        <Card key={index} style={{ 
          marginBottom: '24px',
          border: `2px solid ${sectionPrimaryColor}15`,
          borderRadius: '16px',
          overflow: 'hidden'
        }}>
          {/* Project Header */}
          <div style={{
            background: `linear-gradient(135deg, ${sectionPrimaryColor}08, ${sectionPrimaryColor}15)`,
            padding: '20px',
            borderBottom: `1px solid ${sectionPrimaryColor}20`
          }}>
            <Row align="middle" gutter={[16, 16]}>
              <Col xs={24} sm={16}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                  <div style={{
                    background: sectionPrimaryColor,
                    color: 'white',
                    borderRadius: '8px',
                    width: '40px',
                    height: '40px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '18px',
                    fontWeight: '700'
                  }}>
                    {index + 1}
                  </div>
                  <div>
                    <Title level={3} style={{ 
                      color: sectionPrimaryColor, 
                      marginBottom: '4px',
                      fontSize: '20px'
                    }}>
                      {project.name}
                    </Title>
                    <Text style={{ fontSize: '12px', color: '#666', textTransform: 'uppercase' }}>
                      Project Details
                    </Text>
                  </div>
                </div>
              </Col>
              <Col xs={24} sm={8}>
                <div style={{ textAlign: 'right' }}>
                  <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
                    {project.githubUrl && (
                      <Button 
                        icon={<GithubOutlined />}
                        href={project.githubUrl}
                        target="_blank"
                      >
                        Repository
                      </Button>
                    )}
                    {project.liveUrl && (
                      <Button 
                        type="primary"
                        icon={<LinkOutlined />}
                        href={project.liveUrl}
                        target="_blank"
                      >
                        Live Demo
                      </Button>
                    )}
                  </div>
                </div>
              </Col>
            </Row>
          </div>
          
          {/* Project Content */}
          <div style={{ padding: '24px' }}>
            <Row gutter={[24, 16]}>
              <Col xs={24} md={16}>
                <div>
                  <Title level={5} style={{ 
                    color: '#2c3e50',
                    marginBottom: '12px',
                    fontSize: '14px',
                    textTransform: 'uppercase',
                    letterSpacing: '1px'
                  }}>
                    Description
                  </Title>
                  <Paragraph style={{ 
                    fontSize: '15px', 
                    color: '#666',
                    lineHeight: 1.7,
                    marginBottom: '20px'
                  }}>
                    {project.description}
                  </Paragraph>
                  
                  {project.technologies && (
                    <>
                      <Title level={5} style={{ 
                        color: '#2c3e50',
                        marginBottom: '12px',
                        fontSize: '14px',
                        textTransform: 'uppercase',
                        letterSpacing: '1px'
                      }}>
                        Technologies Used
                      </Title>
                      <div style={{ marginBottom: '20px' }}>
                        {project.technologies.split(',').map((tech: string, techIndex: number) => (
                          <Tag key={techIndex} style={{
                            marginBottom: '6px',
                            padding: '4px 12px',
                            borderRadius: '12px',
                            border: `1px solid ${sectionPrimaryColor}30`,
                            background: `${sectionPrimaryColor}10`,
                            color: sectionPrimaryColor,
                            fontWeight: '500'
                          }}>
                            {tech.trim()}
                          </Tag>
                        ))}
                      </div>
                    </>
                  )}
                </div>
              </Col>
              
              <Col xs={24} md={8}>
                <div style={{
                  background: '#f8f9fa',
                  borderRadius: '12px',
                  padding: '20px',
                  border: `1px solid ${sectionPrimaryColor}15`
                }}>
                  <Title level={5} style={{ 
                    color: '#2c3e50',
                    marginBottom: '16px',
                    fontSize: '14px',
                    textTransform: 'uppercase',
                    letterSpacing: '1px',
                    textAlign: 'center'
                  }}>
                    Project Stats
                  </Title>
                  
                  <div style={{ textAlign: 'center' }}>
                    <div style={{
                      background: `linear-gradient(135deg, ${sectionPrimaryColor}15, ${sectionPrimaryColor}25)`,
                      borderRadius: '12px',
                      width: '80px',
                      height: '80px',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      margin: '0 auto 16px',
                      fontSize: '32px'
                    }}>
                      🚀
                    </div>
                    
                    <div style={{ marginBottom: '12px' }}>
                      <Text style={{ fontSize: '12px', color: '#999', display: 'block' }}>STATUS</Text>
                      <Text style={{ fontSize: '14px', color: sectionPrimaryColor, fontWeight: '600' }}>
                        {project.liveUrl ? 'LIVE' : 'IN DEVELOPMENT'}
                      </Text>
                    </div>
                    
                    <div style={{ marginBottom: '12px' }}>
                      <Text style={{ fontSize: '12px', color: '#999', display: 'block' }}>TECH COUNT</Text>
                      <Text style={{ fontSize: '14px', color: sectionPrimaryColor, fontWeight: '600' }}>
                        {project.technologies ? project.technologies.split(',').length : 0} Technologies
                      </Text>
                    </div>
                    
                    <div>
                      <Text style={{ fontSize: '12px', color: '#999', display: 'block' }}>LINKS</Text>
                      <Text style={{ fontSize: '14px', color: sectionPrimaryColor, fontWeight: '600' }}>
                        {(project.githubUrl ? 1 : 0) + (project.liveUrl ? 1 : 0)} Available
                      </Text>
                    </div>
                  </div>
                </div>
              </Col>
            </Row>
          </div>
        </Card>
      ))}
    </Card>
  );

  // Template selector
  switch (template) {
    case 'grid':
      return renderGridTemplate();
    case 'showcase':
      return renderShowcaseTemplate();
    case 'portfolio':
      return renderPortfolioTemplate();
    case 'cards':
      return renderCardsTemplate();
    case 'detailed':
      return renderDetailedTemplate();
    case 'minimal':
      return renderMinimalTemplate();
    default:
      return renderGridTemplate(); // fallback
  }
};

export default ProjectsSection; 