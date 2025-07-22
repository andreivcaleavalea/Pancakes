import React from 'react';
import { Card, Typography, Row, Col, Tag } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './GridTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface GridTemplateProps {
  projects: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const GridTemplate: React.FC<GridTemplateProps> = ({
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
        🎨 Creative Grid
      </Title>
      
      <Row gutter={[20, 20]}>
        {projects.map((project: any, index: number) => (
          <Col key={index} xs={24} sm={12} md={8} lg={6}>
            <div style={{
              background: '#fff',
              borderRadius: '12px',
              padding: '16px',
              height: '100%',
              border: `1px solid ${sectionPrimaryColor}20`,
              boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
              transition: 'transform 0.3s ease',
              cursor: 'pointer'
            }}>
              <div style={{ textAlign: 'center', marginBottom: '12px' }}>
                <div style={{
                  background: `${sectionPrimaryColor}15`,
                  borderRadius: '8px',
                  width: '40px',
                  height: '40px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  margin: '0 auto',
                  fontSize: '16px'
                }}>
                  🚀
                </div>
              </div>
              
              <Title level={5} style={{ 
                textAlign: 'center',
                color: sectionPrimaryColor,
                marginBottom: '8px',
                fontSize: '14px'
              }}>
                {project.name}
              </Title>
              
              {project.description && (
                <Paragraph style={{ 
                  fontSize: '11px', 
                  color: '#666',
                  lineHeight: 1.4,
                  margin: '0 0 8px',
                  textAlign: 'center'
                }}>
                  {project.description.length > 50 ? project.description.substring(0, 50) + '...' : project.description}
                </Paragraph>
              )}
              
              {project.technologies && (
                <div style={{ textAlign: 'center' }}>
                  <Tag size="small" color={sectionPrimaryColor} style={{ fontSize: '9px' }}>
                    {project.technologies.split(',')[0]?.trim()}
                  </Tag>
                </div>
              )}
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  );
};

export default GridTemplate; 