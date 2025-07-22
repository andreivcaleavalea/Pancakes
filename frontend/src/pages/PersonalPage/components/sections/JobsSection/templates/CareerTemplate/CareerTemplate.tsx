import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './CareerTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface CareerTemplateProps {
  jobs: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const CareerTemplate: React.FC<CareerTemplateProps> = ({
  jobs,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="jobs" className="career-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      <Title level={2} className="career-template__title" style={{ color: sectionPrimaryColor }}>
        🚀 Career Journey
      </Title>
      
      <div className="career-template__timeline">
        {/* Career timeline line */}
        <div 
          className="career-template__timeline-line"
          style={{
            background: `linear-gradient(to bottom, ${sectionPrimaryColor}, ${sectionPrimaryColor}50)`
          }}
        />

        {jobs.map((job: any, index: number) => (
          <div key={index} className="career-template__job-item">
            {/* Career milestone */}
            <div 
              className="career-template__milestone"
              style={{ background: sectionPrimaryColor }}
            />

            <div 
              className="career-template__job-card"
              style={{
                border: `1px solid ${sectionPrimaryColor}20`
              }}
            >
              <Row gutter={[16, 12]}>
                <Col xs={24} sm={18}>
                  <Title level={4} className="career-template__job-title" style={{ color: sectionPrimaryColor }}>
                    {job.position}
                  </Title>
                  <Text strong className="career-template__company">{job.company}</Text>
                  <br />
                  {job.location && (
                    <>
                      <Text className="career-template__location">📍 {job.location}</Text>
                      <br />
                    </>
                  )}
                  <Text type="secondary" className="career-template__dates">
                    {job.startDate} - {job.endDate || 'Present'}
                  </Text>
                  {job.description && (
                    <Paragraph className="career-template__description">
                      {job.description}
                    </Paragraph>
                  )}
                </Col>
                <Col xs={24} sm={6}>
                  <div className="career-template__icon-container">
                    <div 
                      className="career-template__icon"
                      style={{
                        background: `${sectionPrimaryColor}15`
                      }}
                    >
                      💼
                    </div>
                  </div>
                </Col>
              </Row>
            </div>
          </div>
        ))}
      </div>
    </Card>
  );
};

export default CareerTemplate; 