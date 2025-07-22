import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './CardsTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface CardsTemplateProps {
  projects: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const CardsTemplate: React.FC<CardsTemplateProps> = ({
  projects,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="projects" style={{ marginBottom: '24px', position: 'relative' }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor }}>🃏 Project Cards</Title>
       
      <Row gutter={[16, 16]}>
        {projects.map((project: any, index: number) => (
          <Col key={index} xs={24} sm={12} md={8}>
            <div style={{ 
              padding: '16px',
              border: `1px solid ${sectionPrimaryColor}20`,
              borderRadius: '8px',
              height: '100%'
            }}>
              <Text strong style={{ fontSize: '16px' }}>{project.name}</Text>
              <br />
              {project.description && (
                <Paragraph style={{ marginTop: '8px', fontSize: '14px', color: '#666' }}>
                  {project.description}
                </Paragraph>
              )}
              {project.technologies && (
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  Tech: {project.technologies}
                </Text>
              )}
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  );
};

export default CardsTemplate; 