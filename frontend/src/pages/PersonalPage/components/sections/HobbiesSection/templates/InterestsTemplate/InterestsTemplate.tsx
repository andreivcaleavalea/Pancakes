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
    <Card key="hobbies" className="interests-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="interests-template__title" style={{ color: sectionPrimaryColor }}>
        🎯 Interest Cards
      </Title>
       
      <Row gutter={[16, 16]} className="interests-template__grid">
        {hobbies.map((hobby: any, index: number) => (
          <Col key={index} xs={12} sm={8} md={6}>
            <div 
              className="interests-template__interest-card"
              style={{
                background: `${sectionPrimaryColor}10`,
                border: `1px solid ${sectionPrimaryColor}20`
              }}
            >
              <Text className="interests-template__interest-name" style={{ color: sectionPrimaryColor }}>
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