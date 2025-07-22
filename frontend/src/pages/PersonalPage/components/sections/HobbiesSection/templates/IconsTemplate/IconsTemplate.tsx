import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './IconsTemplate.scss';

const { Title, Text } = Typography;

interface IconsTemplateProps {
  hobbies: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const IconsTemplate: React.FC<IconsTemplateProps> = ({
  hobbies,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="hobbies" className="icons-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="icons-template__title" style={{ color: sectionPrimaryColor }}>
        🎪 Icon Gallery
      </Title>
       
      <Row gutter={[16, 16]} className="icons-template__grid">
        {hobbies.map((hobby: any, index: number) => (
          <Col key={index} xs={8} sm={6} md={4}>
            <div 
              className="icons-template__hobby-card"
              style={{
                background: `${sectionPrimaryColor}05`,
                border: `1px solid ${sectionPrimaryColor}15`
              }}
            >
              <div className="icons-template__icon">
                {index === 0 ? '🎨' : index === 1 ? '🎵' : index === 2 ? '🎮' : 
                 index === 3 ? '📚' : index === 4 ? '⚽' : index === 5 ? '🍳' : '🎯'}
              </div>
              <Text className="icons-template__hobby-name" style={{ color: sectionPrimaryColor }}>
                {hobby.name}
              </Text>
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  );
};

export default IconsTemplate; 