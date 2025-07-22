import React from 'react';
import { Card, Typography, Row, Col } from 'antd';
import SectionSettingsPopover from '../../../../SectionSettingsPopover';
import './ProfessionalTemplate.scss';

const { Title, Text, Paragraph } = Typography;

interface ProfessionalTemplateProps {
  jobs: any[];
  sectionKey: string;
  sectionPrimaryColor: string;
  currentSectionSettings: any;
  onSectionSettingsChange: any;
  templateOptions: any;
}

const ProfessionalTemplate: React.FC<ProfessionalTemplateProps> = ({
  jobs,
  sectionKey,
  sectionPrimaryColor,
  currentSectionSettings,
  onSectionSettingsChange,
  templateOptions,
}) => {
  return (
    <Card key="jobs" className="professional-template">
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      {/* Professional Header */}
      <div 
        className="professional-template__header"
        style={{
          background: `linear-gradient(135deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}dd)`
        }}
      >
        <Title level={2} className="professional-template__title">
          👔 Professional Showcase
        </Title>
      </div>
      
      <div className="professional-template__content">
        {jobs.map((job: any, index: number) => (
          <div 
            key={index} 
            className="professional-template__job-item"
            style={{
              borderBottom: index < jobs.length - 1 ? `1px solid ${sectionPrimaryColor}15` : 'none'
            }}
          >
            <Row gutter={[24, 16]}>
              <Col xs={24} sm={18}>
                <div>
                  <Title level={3} className="professional-template__job-position" style={{ color: sectionPrimaryColor }}>
                    {job.position}
                  </Title>
                  
                  <Text className="professional-template__company" style={{ color: sectionPrimaryColor }}>
                    {job.company}
                  </Text>
                  
                  {job.location && (
                    <div className="professional-template__location-container">
                      <span className="professional-template__location-icon" style={{ color: sectionPrimaryColor }}>📍</span>
                      <Text>{job.location}</Text>
                    </div>
                  )}
                  
                  {job.description && (
                    <div 
                      className="professional-template__description-card"
                      style={{
                        borderLeft: `4px solid ${sectionPrimaryColor}`
                      }}
                    >
                      <Paragraph className="professional-template__description">
                        {job.description}
                      </Paragraph>
                    </div>
                  )}
                </div>
              </Col>
              
              <Col xs={24} sm={6}>
                <div className="professional-template__icon-container">
                  <div 
                    className="professional-template__icon"
                    style={{
                      background: `linear-gradient(135deg, ${sectionPrimaryColor}15, ${sectionPrimaryColor}25)`
                    }}
                  >
                    👔
                  </div>
                  <div 
                    className="professional-template__date-badge"
                    style={{
                      background: `${sectionPrimaryColor}10`
                    }}
                  >
                    <Text className="professional-template__date-text" style={{ color: sectionPrimaryColor }}>
                      {job.startDate} - {job.endDate || 'Present'}
                    </Text>
                  </div>
                </div>
              </Col>
            </Row>
          </div>
        ))}
      </div>
    </Card>
  );
};

export default ProfessionalTemplate; 