import React, { useState } from 'react';
import { Card, Form, Input, Button, Select, Space, Typography, Row, Col, message } from 'antd';
import { DragOutlined, EyeInvisibleOutlined, EyeOutlined } from '@ant-design/icons';
import type { PersonalPageSettings, UpdatePersonalPageSettings } from '../../../services/personalPageService';
import { PersonalPageService } from '../../../services/personalPageService';
import './PersonalPageSettings.scss';

const { Title } = Typography;
const { Option } = Select;

interface PersonalPageSettingsProps {
  settings: PersonalPageSettings | null;
  onSettingsChange: (updates: UpdatePersonalPageSettings) => Promise<PersonalPageSettings>;
  onExitSettings: () => void;
}

const PersonalPageSettings: React.FC<PersonalPageSettingsProps> = ({
  settings,
  onSettingsChange,
  onExitSettings
}) => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [sectionOrder, setSectionOrder] = useState<string[]>(
    settings?.sectionOrder || ['personal', 'education', 'jobs', 'projects', 'hobbies']
  );
  const [sectionVisibility, setSectionVisibility] = useState<Record<string, boolean>>(
    settings?.sectionVisibility || {
      personal: true,
      education: true,
      jobs: true,
      projects: true,
      hobbies: true
    }
  );
  const [sectionTemplates, setSectionTemplates] = useState<Record<string, string>>(
    settings?.sectionTemplates || {
      personal: 'card',
      education: 'timeline',
      jobs: 'timeline',
      projects: 'grid',
      hobbies: 'tags'
    }
  );

  const sectionLabels: Record<string, string> = {
    personal: '👤 Personal Info',
    education: '🎓 Education',
    jobs: '💼 Work Experience',
    projects: '🚀 Projects',
    hobbies: '🎯 Hobbies & Interests'
  };

  const templateOptions: Record<string, Array<{ value: string; label: string }>> = {
    personal: [{ value: 'card', label: 'Card Layout' }],
    education: [
      { value: 'timeline', label: 'Timeline' },
      { value: 'cards', label: 'Card Grid' }
    ],
    jobs: [
      { value: 'timeline', label: 'Timeline' },
      { value: 'cards', label: 'Card Grid' }
    ],
    projects: [
      { value: 'grid', label: 'Grid Layout' },
      { value: 'list', label: 'List Layout' }
    ],
    hobbies: [
      { value: 'tags', label: 'Tag Cloud' },
      { value: 'cards', label: 'Card Grid' }
    ]
  };

  const handleSaveSettings = async () => {
    try {
      setLoading(true);
      
      const values = await form.validateFields();
      
      const updates: UpdatePersonalPageSettings = {
        pageSlug: values.pageSlug,
        sectionOrder,
        sectionVisibility,
        sectionTemplates,
        theme: values.theme
      };

      await onSettingsChange(updates);
      message.success('Settings saved successfully!');
    } catch (error) {
      console.error('Error saving settings:', error);
      message.error('Failed to save settings');
    } finally {
      setLoading(false);
    }
  };

  const handleGenerateSlug = async () => {
    try {
      const slug = await PersonalPageService.generateSlug(settings?.pageSlug || 'my-page');
      form.setFieldsValue({ pageSlug: slug });
      message.success('New slug generated!');
    } catch (error) {
      console.error('Error generating slug:', error);
      message.error('Failed to generate slug');
    }
  };

  const toggleSectionVisibility = (section: string) => {
    setSectionVisibility(prev => ({
      ...prev,
      [section]: !prev[section]
    }));
  };

  const moveSectionUp = (index: number) => {
    if (index > 0) {
      const newOrder = [...sectionOrder];
      [newOrder[index - 1], newOrder[index]] = [newOrder[index], newOrder[index - 1]];
      setSectionOrder(newOrder);
    }
  };

  const moveSectionDown = (index: number) => {
    if (index < sectionOrder.length - 1) {
      const newOrder = [...sectionOrder];
      [newOrder[index], newOrder[index + 1]] = [newOrder[index + 1], newOrder[index]];
      setSectionOrder(newOrder);
    }
  };

  return (
    <div className="personal-page-settings">
      <Row gutter={24}>
        <Col xs={24} lg={12}>
          <Card title="Page Settings" className="personal-page-settings__card">
            <Form
              form={form}
              layout="vertical"
              initialValues={{
                pageSlug: settings?.pageSlug,
                theme: settings?.theme || 'modern'
              }}
            >
              <Form.Item
                label="Page URL"
                name="pageSlug"
                rules={[
                  { required: true, message: 'Page URL is required' },
                  { pattern: /^[a-zA-Z0-9\-_]+$/, message: 'Only letters, numbers, hyphens and underscores allowed' }
                ]}
                extra="This will be your public page URL: pancakes.dev/personal/your-slug"
              >
                <Input
                  placeholder="my-awesome-page"
                  addonAfter={
                    <Button type="link" size="small" onClick={handleGenerateSlug}>
                      Generate
                    </Button>
                  }
                />
              </Form.Item>

              <Form.Item
                label="Theme"
                name="theme"
              >
                <Select>
                  <Option value="modern">Modern</Option>
                  <Option value="classic">Classic</Option>
                  <Option value="minimal">Minimal</Option>
                </Select>
              </Form.Item>
            </Form>
          </Card>
        </Col>

        <Col xs={24} lg={12}>
          <Card title="Section Order & Visibility" className="personal-page-settings__card">
            <div className="personal-page-settings__sections">
              {sectionOrder.map((section, index) => (
                <div key={section} className="personal-page-settings__section-item">
                  <div className="personal-page-settings__section-content">
                    <div className="personal-page-settings__section-info">
                      <DragOutlined className="personal-page-settings__drag-handle" />
                      <span className="personal-page-settings__section-label">
                        {sectionLabels[section]}
                      </span>
                    </div>
                    
                    <Space>
                      <Select
                        value={sectionTemplates[section]}
                        onChange={(value) => setSectionTemplates(prev => ({ ...prev, [section]: value }))}
                        size="small"
                        style={{ width: 120 }}
                      >
                        {templateOptions[section]?.map(option => (
                          <Option key={option.value} value={option.value}>
                            {option.label}
                          </Option>
                        ))}
                      </Select>
                      
                      <Button
                        size="small"
                        icon={sectionVisibility[section] ? <EyeOutlined /> : <EyeInvisibleOutlined />}
                        onClick={() => toggleSectionVisibility(section)}
                        type={sectionVisibility[section] ? "default" : "dashed"}
                      />
                      
                      <Space.Compact>
                        <Button
                          size="small"
                          onClick={() => moveSectionUp(index)}
                          disabled={index === 0}
                        >
                          ↑
                        </Button>
                        <Button
                          size="small"
                          onClick={() => moveSectionDown(index)}
                          disabled={index === sectionOrder.length - 1}
                        >
                          ↓
                        </Button>
                      </Space.Compact>
                    </Space>
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </Col>
      </Row>

      <div className="personal-page-settings__actions">
        <Space>
          <Button onClick={onExitSettings}>
            Cancel
          </Button>
          <Button 
            type="primary" 
            onClick={handleSaveSettings}
            loading={loading}
          >
            Save Settings
          </Button>
        </Space>
      </div>
    </div>
  );
};

export default PersonalPageSettings; 