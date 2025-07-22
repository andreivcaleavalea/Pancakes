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
    <Card key="hobbies" style={{ marginBottom: '24px', position: 'relative' }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} style={{ color: sectionPrimaryColor }}>🎪 Icon Gallery</Title>
       
      <Row gutter={[16, 16]} style={{ marginTop: '16px' }}>
        {hobbies.map((hobby: any, index: number) => (
          <Col key={index} xs={8} sm={6} md={4}>
            <div style={{
              textAlign: 'center',
              padding: '16px',
              borderRadius: '12px',
              background: `${sectionPrimaryColor}05`,
              border: `1px solid ${sectionPrimaryColor}15`
            }}>
              <div style={{
                fontSize: '32px',
                marginBottom: '8px'
              }}>
                {index === 0 ? '🎨' : index === 1 ? '🎵' : index === 2 ? '🎮' : 
                 index === 3 ? '📚' : index === 4 ? '⚽' : index === 5 ? '🍳' : '🎯'}
              </div>
              <Text style={{ 
                fontSize: '12px', 
                color: sectionPrimaryColor,
                fontWeight: '500'
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

export default IconsTemplate; 