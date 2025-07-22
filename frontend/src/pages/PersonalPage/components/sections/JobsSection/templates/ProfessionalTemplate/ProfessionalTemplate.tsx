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
    <Card key="jobs" style={{ 
      marginBottom: '32px',
      position: 'relative',
      background: '#fff',
      borderRadius: '20px',
      boxShadow: '0 20px 40px rgba(0,0,0,0.1)',
      overflow: 'hidden'
    }}>
      <SectionSettingsPopover
        sectionKey={sectionKey}
        sectionSettings={currentSectionSettings}
        onSettingsChange={onSectionSettingsChange}
        templateOptions={templateOptions}
      />
      
      {/* Professional Header */}
      <div style={{
        background: `linear-gradient(135deg, ${sectionPrimaryColor}, ${sectionPrimaryColor}dd)`,
        height: '120px',
        position: 'relative',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center'
      }}>
        <Title level={2} style={{ color: 'white', margin: 0, fontSize: '24px', fontWeight: '600' }}>
          👔 Professional Showcase
        </Title>
      </div>
      
      <div style={{ padding: '40px' }}>
        {jobs.map((job: any, index: number) => (
          <div key={index} style={{
            marginBottom: index < jobs.length - 1 ? '32px' : 0,
            paddingBottom: index < jobs.length - 1 ? '32px' : 0,
            borderBottom: index < jobs.length - 1 ? `1px solid ${sectionPrimaryColor}15` : 'none'
          }}>
            <Row gutter={[24, 16]}>
              <Col xs={24} sm={18}>
                <div>
                  <Title level={3} style={{ 
                    color: sectionPrimaryColor, 
                    marginBottom: '8px',
                    fontWeight: '600'
                  }}>
                    {job.position}
                  </Title>
                  
                  <Text style={{ 
                    fontSize: '16px', 
                    color: sectionPrimaryColor,
                    fontWeight: '500',
                    display: 'block',
                    marginBottom: '12px'
                  }}>
                    {job.company}
                  </Text>
                  
                  {job.location && (
                    <div style={{ 
                      display: 'flex', 
                      alignItems: 'center', 
                      marginBottom: '12px' 
                    }}>
                      <span style={{ 
                        fontSize: '16px', 
                        marginRight: '12px',
                        color: sectionPrimaryColor
                      }}>📍</span>
                      <Text>{job.location}</Text>
                    </div>
                  )}
                  
                  {job.description && (
                    <div style={{ 
                      marginTop: '16px',
                      padding: '20px',
                      background: '#f8f9fa',
                      borderRadius: '12px',
                      borderLeft: `4px solid ${sectionPrimaryColor}`
                    }}>
                      <Paragraph style={{ 
                        fontSize: '15px', 
                        lineHeight: 1.7,
                        color: '#555',
                        margin: 0
                      }}>
                        {job.description}
                      </Paragraph>
                    </div>
                  )}
                </div>
              </Col>
              
              <Col xs={24} sm={6}>
                <div style={{ textAlign: 'center' }}>
                  <div style={{
                    background: `linear-gradient(135deg, ${sectionPrimaryColor}15, ${sectionPrimaryColor}25)`,
                    borderRadius: '50%',
                    width: '80px',
                    height: '80px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    margin: '0 auto 16px',
                    fontSize: '28px'
                  }}>
                    👔
                  </div>
                  <div style={{
                    background: `${sectionPrimaryColor}10`,
                    padding: '8px 16px',
                    borderRadius: '20px',
                    display: 'inline-block'
                  }}>
                    <Text style={{ fontSize: '12px', color: sectionPrimaryColor, fontWeight: '600' }}>
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