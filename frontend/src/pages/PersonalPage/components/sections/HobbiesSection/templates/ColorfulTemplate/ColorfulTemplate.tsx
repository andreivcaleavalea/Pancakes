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
    <Card key="hobbies" className="colorful-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="colorful-template__title" style={{ color: sectionPrimaryColor }}>
        🌈 Colorful Display
      </Title>
      
      <Row gutter={[16, 16]}>
        {hobbies.map((hobby: any, index: number) => (
          <Col key={index} xs={12} sm={8} md={6} lg={4}>
            <div 
              className="colorful-template__hobby-card"
              style={{ background: colors[index % colors.length] }}
            >
              <div className="colorful-template__icon">
                {index === 0 ? '🎨' : index === 1 ? '🎵' : index === 2 ? '🎮' : index === 3 ? '📚' : 
                 index === 4 ? '⚽' : index === 5 ? '🍳' : index === 6 ? '📷' : '🎯'}
              </div>
              <Text className="colorful-template__hobby-name">
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