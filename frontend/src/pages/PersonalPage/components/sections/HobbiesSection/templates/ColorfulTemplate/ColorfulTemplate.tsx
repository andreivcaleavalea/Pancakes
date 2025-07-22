import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './ColorfulTemplate.scss';

const { Title, Text } = Typography;

interface ColorfulTemplateProps {
  hobbies: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const ColorfulTemplate: React.FC<ColorfulTemplateProps> = ({
  hobbies,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  const colors = ['#ff6b6b', '#4ecdc4', '#45b7d1', '#96ceb4', '#ffeaa7', '#dda0dd', '#98d8c8', '#f7dc6f'];

  return (
    <Card key="hobbies" style={{ 
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
        🌈 Colorful Display
      </Title>
      
      <Row gutter={[16, 16]}>
        {hobbies.map((hobby: any, index: number) => (
          <Col key={index} xs={12} sm={8} md={6} lg={4}>
            <div style={{
              background: colors[index % colors.length],
              borderRadius: '16px',
              padding: '16px',
              textAlign: 'center',
              color: 'white',
              boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
              transition: 'transform 0.3s ease'
            }}>
              <div style={{
                fontSize: '24px',
                marginBottom: '8px'
              }}>
                {index === 0 ? '🎨' : index === 1 ? '🎵' : index === 2 ? '🎮' : index === 3 ? '📚' : 
                 index === 4 ? '⚽' : index === 5 ? '🍳' : index === 6 ? '📷' : '🎯'}
              </div>
              <Text style={{ 
                color: 'white', 
                fontSize: '12px',
                fontWeight: '600'
              }}>
                {hobby.name}
              </Text>
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  );
};

export default ColorfulTemplate; 