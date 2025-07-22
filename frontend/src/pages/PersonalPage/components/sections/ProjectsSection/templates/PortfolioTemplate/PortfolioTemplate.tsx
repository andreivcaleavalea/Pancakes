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
    <Card key="projects" style={{ 
      marginBottom: '32px',
      position: 'relative',
      borderRadius: '20px',
      background: 'linear-gradient(135deg, #f8f9fa 0%, #ffffff 100%)',
      border: 'none',
      boxShadow: '0 15px 35px rgba(0,0,0,0.1)'
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
        padding: '40px',
        textAlign: 'center',
        borderRadius: '20px 20px 0 0',
        position: 'relative'
      }}>
        <div style={{
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'url("data:image/svg+xml,%3Csvg width="60" height="60" viewBox="0 0 60 60" xmlns="http://www.w3.org/2000/svg"%3E%3Cg fill="none" fill-rule="evenodd"%3E%3Cg fill="%23ffffff" fill-opacity="0.1"%3E%3Cpath d="M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4z"/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")',
          opacity: 0.3
        }} />
        <Title level={2} style={{ color: 'white', margin: 0, fontSize: '32px', fontWeight: '700' }}>
          🖼️ Portfolio Gallery
        </Title>
        <Text style={{ color: 'rgba(255,255,255,0.9)', fontSize: '18px', marginTop: '12px', display: 'block' }}>
          Showcasing Creative Projects
        </Text>
      </div>
      
      <div style={{ padding: '40px' }}>
        <Row gutter={[32, 32]}>
          {projects.map((project: any, index: number) => (
            <Col key={index} xs={24} md={12} lg={8}>
              <div style={{
                position: 'relative',
                background: '#fff',
                borderRadius: '20px',
                overflow: 'hidden',
                boxShadow: '0 10px 30px rgba(0,0,0,0.1)',
                transition: 'transform 0.3s ease, box-shadow 0.3s ease',
                height: '100%'
              }}>
                {/* Project Image Placeholder */}
                <div style={{
                  height: '200px',
                  background: `linear-gradient(135deg, ${sectionPrimaryColor}20, ${sectionPrimaryColor}40)`,
                  position: 'relative',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center'
                }}>
                  <div style={{
                    fontSize: '48px',
                    opacity: 0.8
                  }}>
                    {index === 0 ? '🎨' : index === 1 ? '💻' : index === 2 ? '📱' : '🚀'}
                  </div>
                  
                  {/* Project Badge */}
                  <div style={{
                    position: 'absolute',
                    top: '16px',
                    right: '16px',
                    background: 'rgba(255,255,255,0.9)',
                    padding: '6px 12px',
                    borderRadius: '20px',
                    backdropFilter: 'blur(10px)'
                  }}>
                    <Text style={{ fontSize: '11px', color: sectionPrimaryColor, fontWeight: '600' }}>
                      PROJECT #{index + 1}
                    </Text>
                  </div>
                </div>
                
                <div style={{ padding: '24px' }}>
                  <Title level={4} style={{ 
                    color: sectionPrimaryColor, 
                    marginBottom: '12px',
                    fontSize: '18px'
                  }}>
                    {project.name}
                  </Title>
                  
                  {project.description && (
                    <Paragraph style={{ 
                      fontSize: '14px', 
                      color: '#666',
                      lineHeight: 1.6,
                      marginBottom: '16px'
                    }}>
                      {project.description.length > 100 ? project.description.substring(0, 100) + '...' : project.description}
                    </Paragraph>
                  )}
                  
                  {/* Tech Stack */}
                  {project.technologies && (
                    <div style={{ marginBottom: '16px' }}>
                      <Text style={{ fontSize: '12px', color: '#999', marginBottom: '8px', display: 'block' }}>
                        TECH STACK:
                      </Text>
                      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px' }}>
                        {project.technologies.split(',').slice(0, 3).map((tech: string, techIndex: number) => (
                          <Tag 
                            key={techIndex} 
                            size="small" 
                            color={sectionPrimaryColor}
                            style={{ fontSize: '10px' }}
                          >
                            {tech.trim()}
                          </Tag>
                        ))}
                        {project.technologies.split(',').length > 3 && (
                          <Tag size="small" style={{ fontSize: '10px', color: '#999' }}>
                            +{project.technologies.split(',').length - 3}
                          </Tag>
                        )}
                      </div>
                    </div>
                  )}
                  
                  {/* Project Links */}
                  <div style={{ 
                    display: 'flex', 
                    gap: '8px',
                    justifyContent: 'space-between',
                    alignItems: 'center'
                  }}>
                    {project.startDate && (
                      <Text style={{ fontSize: '11px', color: '#999' }}>
                        {project.startDate}
                      </Text>
                    )}
                    <div style={{ display: 'flex', gap: '8px' }}>
                      {project.demoUrl && (
                        <div style={{
                          background: `${sectionPrimaryColor}15`,
                          padding: '4px 8px',
                          borderRadius: '6px'
                        }}>
                          <Text style={{ fontSize: '10px', color: sectionPrimaryColor, fontWeight: '600' }}>
                            🔗 DEMO
                          </Text>
                        </div>
                      )}
                      {project.sourceUrl && (
                        <div style={{
                          background: '#333',
                          padding: '4px 8px',
                          borderRadius: '6px'
                        }}>
                          <Text style={{ fontSize: '10px', color: 'white', fontWeight: '600' }}>
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