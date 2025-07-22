import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './InterestsTemplate.scss';

const { Title, Text } = Typography;

interface InterestsTemplateProps {
  hobbies: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const InterestsTemplate: React.FC<InterestsTemplateProps> = ({
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
      
      <Title level={2} style={{ color: sectionPrimaryColor }}>🎯 Interest Cards</Title>
       
      <Row gutter={[16, 16]} style={{ marginTop: '16px' }}>
        {hobbies.map((hobby: any, index: number) => (
          <Col key={index} xs={12} sm={8} md={6}>
            <div style={{
              background: `${sectionPrimaryColor}10`,
              padding: '12px',
              borderRadius: '8px',
              textAlign: 'center',
              border: `1px solid ${sectionPrimaryColor}20`
            }}>
              <Text style={{ fontSize: '14px', color: sectionPrimaryColor, fontWeight: '500' }}>
                {hobby.name}
              </Text>
            </div>
          </Col>
        ))}
      </Row>
    </Card>
  );
};

export default InterestsTemplate; 