import React, { useCallback } from 'react';
import { Popover, Button, Form, Select, App, Collapse, Switch, Slider, Input, ColorPicker } from 'antd';
import { SettingOutlined, SettingFilled } from '@ant-design/icons';
import type { SectionSettingsPopoverProps } from '../types';
import type { AdvancedSectionSettings } from '../../../services/personalPageService';
import { SECTION_COLORS } from '../constants';



// Helper functions to convert string values to numeric for sliders
const getFontSizeNumeric = (fontSize: string): number => {
  switch (fontSize) {
    case 'small': return 14;
    case 'medium': return 16;
    case 'large': return 18;
    case 'xl': return 20;
    default: return 16;
  }
};

const getFontWeightNumeric = (fontWeight: string): number => {
  switch (fontWeight) {
    case 'normal': return 400;
    case 'medium': return 500;
    case 'semibold': return 600;
    case 'bold': return 700;
    default: return 400;
  }
};

const getShadowIntensityNumeric = (intensity: string): number => {
  switch (intensity) {
    case 'light': return 1;
    case 'medium': return 2;
    case 'strong': return 3;
    default: return 2;
  }
};

const SectionSettingsPopover: React.FC<SectionSettingsPopoverProps> = ({
  sectionKey,
  sectionSettings,
  onSettingsChange,
  templateOptions,
  editMode = true,
}) => {
  
  // Don't render anything in public view
  if (!editMode) {
    return null;
  }
  const { message } = App.useApp();
  const [form] = Form.useForm();

  // Default advanced settings
  const defaultAdvancedSettings: AdvancedSectionSettings = {
    layout: {
      fullscreen: false,
      margin: 16
    },
    background: {
      color: "",
      pattern: "none",
      opacity: 1.0
    },
    typography: {
      fontSize: "medium",
      fontColor: "",
      fontWeight: "normal"
    },
    styling: {
      roundCorners: true,
      borderRadius: "8px",
      shadow: true,
      shadowIntensity: "medium",
      border: {
        enabled: false,
        color: "#e5e7eb",
        width: "1px",
        style: "solid"
      }
    }
  };

  const currentAdvancedSettings = sectionSettings.advancedSettings || defaultAdvancedSettings;

  const getEffectiveValue = (newValue: any, currentValue: any) => {
    if (newValue === null || newValue === undefined) {
      return currentValue || "";
    }
    if (typeof newValue === 'string' && newValue.trim() === '') {
      return currentValue || "";
    }
    return newValue;
  };

  const handleChange = useCallback((field: string, value: string | number | boolean) => {
    setTimeout(() => {
      const currentValues = form.getFieldsValue();
      const updatedValues = { ...currentValues, [field]: value };
      
      const finalBackground = {
        color: field === 'backgroundColor' 
          ? (value as string || "")  
          : (currentAdvancedSettings.background.color || ""),  
        pattern: field === 'backgroundPattern'
          ? (value as string || "none") 
          : (currentAdvancedSettings.background.pattern || "none"),  
        opacity: field === 'backgroundOpacity'
          ? (value as number || 1.0)  
          : (currentAdvancedSettings.background.opacity || 1.0)  
      };
    

      
      const updatedSettings = {
        template: updatedValues.template || sectionSettings.template,
        color: updatedValues.color || sectionSettings.color,
        advancedSettings: {
          layout: {
            fullscreen: getEffectiveValue(updatedValues.fullscreen, currentAdvancedSettings.layout.fullscreen),
            margin: getEffectiveValue(updatedValues.margin, currentAdvancedSettings.layout.margin)
          },
          background: finalBackground,
          typography: {
            fontSize: updatedValues.fontSize !== undefined ? convertFontSizeToString(updatedValues.fontSize) : currentAdvancedSettings.typography.fontSize,
            fontColor: getEffectiveValue(updatedValues.fontColor, currentAdvancedSettings.typography.fontColor),
            fontWeight: updatedValues.fontWeight !== undefined ? convertFontWeightToString(updatedValues.fontWeight) : currentAdvancedSettings.typography.fontWeight
          },
          styling: {
            roundCorners: getEffectiveValue(updatedValues.roundCorners, currentAdvancedSettings.styling.roundCorners),
            borderRadius: getEffectiveValue(updatedValues.borderRadius, currentAdvancedSettings.styling.borderRadius),
            shadow: getEffectiveValue(updatedValues.shadow, currentAdvancedSettings.styling.shadow),
            shadowIntensity: updatedValues.shadowIntensity !== undefined ? convertShadowIntensityToString(updatedValues.shadowIntensity) : currentAdvancedSettings.styling.shadowIntensity,
            border: {
              enabled: getEffectiveValue(updatedValues.borderEnabled, currentAdvancedSettings.styling.border.enabled),
              color: getEffectiveValue(updatedValues.borderColor, currentAdvancedSettings.styling.border.color),
              width: getEffectiveValue(updatedValues.borderWidth, currentAdvancedSettings.styling.border.width),
              style: getEffectiveValue(updatedValues.borderStyle, currentAdvancedSettings.styling.border.style)
            }
          }
        }
      };
      
      onSettingsChange(sectionKey, updatedSettings);
    }, 16);
  }, [form, currentAdvancedSettings, sectionSettings, sectionKey, onSettingsChange]);

  const convertFontSizeToString = (size: number): string => {
    if (size <= 14) return 'small';
    if (size <= 16) return 'medium';
    if (size <= 18) return 'large';
    return 'xl';
  };

  const convertFontWeightToString = (weight: number): string => {
    if (weight <= 400) return 'normal';
    if (weight <= 500) return 'medium';
    if (weight <= 600) return 'semibold';
    return 'bold';
  };

  const convertShadowIntensityToString = (intensity: number): string => {
    if (intensity <= 1) return 'light';
    if (intensity <= 2) return 'medium';
    return 'strong';
  };



  const initialValues = {
    template: sectionSettings.template,
    color: sectionSettings.color,
    // Layout
    fullscreen: currentAdvancedSettings.layout.fullscreen,
    margin: typeof currentAdvancedSettings.layout.margin === 'string' ? 16 : currentAdvancedSettings.layout.margin,
    // Background
    backgroundColor: currentAdvancedSettings.background.color,
    backgroundPattern: currentAdvancedSettings.background.pattern,
    backgroundOpacity: currentAdvancedSettings.background.opacity,
    // Typography - convert to numeric values
    fontSize: getFontSizeNumeric(currentAdvancedSettings.typography.fontSize),
    fontColor: currentAdvancedSettings.typography.fontColor,
    fontWeight: getFontWeightNumeric(currentAdvancedSettings.typography.fontWeight),
    // Styling
    roundCorners: currentAdvancedSettings.styling.roundCorners,
    borderRadius: currentAdvancedSettings.styling.borderRadius,
    shadow: currentAdvancedSettings.styling.shadow,
    shadowIntensity: getShadowIntensityNumeric(currentAdvancedSettings.styling.shadowIntensity),
    borderEnabled: currentAdvancedSettings.styling.border.enabled,
    borderColor: currentAdvancedSettings.styling.border.color,
    borderWidth: currentAdvancedSettings.styling.border.width,
    borderStyle: currentAdvancedSettings.styling.border.style,
  };

  const content = (
    <div style={{ minWidth: '350px', maxHeight: '500px', overflowY: 'auto' }}>
      <Form
        form={form}
        layout="vertical"
        initialValues={initialValues}
        size="small"
      >
        {/* Basic Settings */}
        <Form.Item name="template" label="Template">
          <Select 
            placeholder="Choose template" 
            options={templateOptions}
            onChange={(value) => handleChange('template', value)}
          />
        </Form.Item>
        
        <Form.Item name="color" label="Color Scheme">
          <Select 
            placeholder="Choose color"
            onChange={(value) => handleChange('color', value)}
          >
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

        {/* Advanced Settings */}
        <Collapse 
          size="small" 
          ghost
          items={[
            {
              key: 'advanced',
              label: '🎨 Advanced Settings',
              children: (
                <>
                  {/* Layout Settings */}
                  <div style={{ marginBottom: '16px' }}>
                    <h4 style={{ margin: '0 0 8px 0', fontSize: '12px', fontWeight: 600 }}>Layout</h4>
                    <Form.Item name="fullscreen" valuePropName="checked" style={{ marginBottom: '8px' }}>
                      <Switch 
                        size="small" 
                        checkedChildren="Fullscreen" 
                        unCheckedChildren="Card"
                        onChange={(value) => handleChange('fullscreen', value)}
                      />
                    </Form.Item>
                    <Form.Item name="margin" label="Margin (px)" style={{ marginBottom: '8px' }}>
                      <Slider 
                        min={0} 
                        max={100} 
                        marks={{ 0: '0px', 16: '16px', 32: '32px', 64: '64px', 100: '100px' }}
                        onChange={(value) => handleChange('margin', value)}
                      />
                    </Form.Item>
                  </div>

                  {/* Background Settings */}
                  <div style={{ marginBottom: '16px' }}>
                    <h4 style={{ margin: '0 0 8px 0', fontSize: '12px', fontWeight: 600 }}>Background</h4>
                    <Form.Item name="backgroundColor" label="Background Color" style={{ marginBottom: '8px' }}>
                      <ColorPicker 
                        size="small"
                        showText
                        allowClear
                        onChange={(color) => handleChange('backgroundColor', color?.toHexString() || '')}
                      />
                    </Form.Item>
                    <Form.Item name="backgroundPattern" label="Pattern" style={{ marginBottom: '8px' }}>
                      <Select 
                        size="small"
                        onChange={(value) => handleChange('backgroundPattern', value)}
                      >
                        <Select.Option value="none">None</Select.Option>
                        <Select.Option value="dots">Dots</Select.Option>
                        <Select.Option value="grid">Grid</Select.Option>
                        <Select.Option value="diagonal">Diagonal</Select.Option>
                        <Select.Option value="waves">Waves</Select.Option>
                      </Select>
                    </Form.Item>
                    <Form.Item name="backgroundOpacity" label="Pattern Opacity" style={{ marginBottom: '8px' }}>
                      <Slider 
                        min={0} 
                        max={1} 
                        step={0.1} 
                        marks={{ 0: '0%', 0.5: '50%', 1: '100%' }}
                        onChange={(value) => handleChange('backgroundOpacity', value)}
                      />
                    </Form.Item>
                  </div>

                  {/* Typography Settings */}
                  <div style={{ marginBottom: '16px' }}>
                    <h4 style={{ margin: '0 0 8px 0', fontSize: '12px', fontWeight: 600 }}>Typography</h4>
                    <Form.Item name="fontSize" label="Font Size (px)" style={{ marginBottom: '8px' }}>
                      <Slider 
                        min={12} 
                        max={24} 
                        marks={{ 12: '12px', 14: '14px', 16: '16px', 18: '18px', 20: '20px', 24: '24px' }}
                        onChange={(value) => handleChange('fontSize', value)}
                      />
                    </Form.Item>
                    <Form.Item name="fontColor" label="Font Color" style={{ marginBottom: '8px' }}>
                      <ColorPicker 
                        size="small"
                        showText
                        allowClear
                        onChange={(color) => handleChange('fontColor', color?.toHexString() || '')}
                      />
                    </Form.Item>
                    <Form.Item name="fontWeight" label="Font Weight" style={{ marginBottom: '8px' }}>
                      <Slider 
                        min={400} 
                        max={700} 
                        step={100}
                        marks={{ 400: 'Normal', 500: 'Medium', 600: 'Semi', 700: 'Bold' }}
                        onChange={(value) => handleChange('fontWeight', value)}
                      />
                    </Form.Item>
                  </div>

                  {/* Styling Settings */}
                  <div style={{ marginBottom: '16px' }}>
                    <h4 style={{ margin: '0 0 8px 0', fontSize: '12px', fontWeight: 600 }}>Styling</h4>
                    <Form.Item name="roundCorners" valuePropName="checked" style={{ marginBottom: '8px' }}>
                      <Switch 
                        size="small" 
                        checkedChildren="Round Corners" 
                        unCheckedChildren="Sharp Corners"
                        onChange={(value) => handleChange('roundCorners', value)}
                      />
                    </Form.Item>
                    <Form.Item name="borderRadius" label="Border Radius" style={{ marginBottom: '8px' }}>
                      <Select 
                        size="small"
                        onChange={(value) => handleChange('borderRadius', value)}
                      >
                        <Select.Option value="4px">Small (4px)</Select.Option>
                        <Select.Option value="8px">Medium (8px)</Select.Option>
                        <Select.Option value="12px">Large (12px)</Select.Option>
                        <Select.Option value="16px">Extra Large (16px)</Select.Option>
                        <Select.Option value="24px">Huge (24px)</Select.Option>
                      </Select>
                    </Form.Item>
                    <Form.Item name="shadow" valuePropName="checked" style={{ marginBottom: '8px' }}>
                      <Switch 
                        size="small" 
                        checkedChildren="Shadow On" 
                        unCheckedChildren="Shadow Off"
                        onChange={(value) => handleChange('shadow', value)}
                      />
                    </Form.Item>
                    <Form.Item name="shadowIntensity" label="Shadow Intensity" style={{ marginBottom: '8px' }}>
                      <Slider 
                        min={1} 
                        max={3} 
                        marks={{ 1: 'Light', 2: 'Medium', 3: 'Strong' }}
                        onChange={(value) => handleChange('shadowIntensity', value)}
                      />
                    </Form.Item>
                  </div>
                </>
              )
            }
          ]}
        />
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