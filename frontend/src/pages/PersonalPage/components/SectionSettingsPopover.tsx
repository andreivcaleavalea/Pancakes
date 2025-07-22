import React from 'react';
import { Popover, Button, Form, Select, App } from 'antd';
import { SettingOutlined } from '@ant-design/icons';
import type { SectionSettingsPopoverProps } from '../types';
import { SECTION_COLORS } from '../constants';

const SectionSettingsPopover: React.FC<SectionSettingsPopoverProps> = ({
  sectionKey,
  sectionSettings,
  onSettingsChange,
  templateOptions,
}) => {
  const { message } = App.useApp();

  const handleSettingsSubmit = (values: any) => {
    onSettingsChange(sectionKey, values);
    message.success('Section settings updated!');
  };

  const content = (
    <div style={{ minWidth: '250px' }}>
      <Form
        layout="vertical"
        initialValues={sectionSettings}
        onFinish={handleSettingsSubmit}
        size="small"
      >
        <Form.Item name="template" label="Template">
          <Select
            placeholder="Choose template"
            options={templateOptions}
          />
        </Form.Item>
        
        <Form.Item name="color" label="Color Scheme">
          <Select placeholder="Choose color">
            {Object.entries(SECTION_COLORS).map(([key, color]) => (
              <Select.Option key={key} value={key}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <div
                    style={{
                      width: '12px',
                      height: '12px',
                      borderRadius: '50%',
                      backgroundColor: color,
                    }}
                  />
                  <span style={{ textTransform: 'capitalize' }}>{key}</span>
                </div>
              </Select.Option>
            ))}
          </Select>
        </Form.Item>
        
        <Form.Item style={{ marginBottom: 0 }}>
          <Button type="primary" htmlType="submit" size="small" block>
            Apply Settings
          </Button>
        </Form.Item>
      </Form>
    </div>
  );

  return (
    <Popover
      content={content}
      title={`${sectionKey.charAt(0).toUpperCase() + sectionKey.slice(1)} Section Settings`}
      trigger="click"
      placement="bottomRight"
    >
      <Button
        type="text"
        icon={<SettingOutlined />}
        size="small"
        style={{
          position: 'absolute',
          top: '16px',
          right: '16px',
          zIndex: 10,
          opacity: 0.6,
        }}
        className="section-settings-btn"
      />
    </Popover>
  );
};

export default SectionSettingsPopover; 