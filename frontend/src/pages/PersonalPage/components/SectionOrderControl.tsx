import React, { useState } from 'react';
import { Card, Button, Typography, Space, Tooltip, message, Switch } from 'antd';
import { ArrowUpOutlined, ArrowDownOutlined, MenuOutlined, ReloadOutlined, SettingOutlined, EyeOutlined, EyeInvisibleOutlined } from '@ant-design/icons';

const { Text, Title } = Typography;

interface SectionOrderControlProps {
  sectionOrder: string[];
  onSectionOrderChange: (newOrder: string[]) => void;
  sectionVisibility: Record<string, boolean>;
  onSectionVisibilityChange: (sectionKey: string, visible: boolean) => void;
}

const SECTION_LABELS = {
  personal: 'Personal Info',
  education: 'Education',
  jobs: 'Work Experience', 
  projects: 'Projects',
  hobbies: 'Hobbies & Interests'
};

const SECTION_ICONS = {
  personal: '👤',
  education: '🎓',
  jobs: '💼',
  projects: '🚀',
  hobbies: '🎨'
};

const SectionOrderControl: React.FC<SectionOrderControlProps> = ({
  sectionOrder,
  onSectionOrderChange,
  sectionVisibility,
  onSectionVisibilityChange
}) => {
  const [isExpanded, setIsExpanded] = useState(false);

  const moveSection = (fromIndex: number, toIndex: number) => {
    if (toIndex < 0 || toIndex >= sectionOrder.length) return;
    
    const newOrder = [...sectionOrder];
    const [moved] = newOrder.splice(fromIndex, 1);
    newOrder.splice(toIndex, 0, moved);
    
    onSectionOrderChange(newOrder);
    message.success(`Moved ${SECTION_LABELS[moved as keyof typeof SECTION_LABELS]} to position ${toIndex + 1}`);
  };

  const moveUp = (index: number) => {
    moveSection(index, index - 1);
  };

  const moveDown = (index: number) => {
    moveSection(index, index + 1);
  };

  const resetToDefault = () => {
    const defaultOrder = ['personal', 'education', 'jobs', 'projects', 'hobbies'];
    onSectionOrderChange(defaultOrder);
    message.success('Reset to default section order');
  };

  const toggleVisibility = (sectionKey: string) => {
    const newVisibility = !sectionVisibility[sectionKey];
    onSectionVisibilityChange(sectionKey, newVisibility);
    message.success(`${SECTION_LABELS[sectionKey as keyof typeof SECTION_LABELS]} ${newVisibility ? 'shown' : 'hidden'}`);
  };

  const visibleCount = Object.values(sectionVisibility).filter(Boolean).length;

  return (
    <div className="section-order-control" style={{ marginBottom: '16px' }}>
      {/* Collapsible Button */}
      <Button
        type="default"
        size="middle"
        onClick={() => setIsExpanded(!isExpanded)}
        style={{ 
          width: '100%', 
          height: 'auto',
          padding: '12px 16px',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          border: '1px solid #d9d9d9',
          borderRadius: '6px',
          marginBottom: isExpanded ? '8px' : '0'
        }}
      >
        <Space>
          <SettingOutlined />
          <span style={{ fontWeight: 500 }}>Section Order & Visibility</span>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            ({visibleCount}/{sectionOrder.length} visible)
          </Text>
        </Space>
        <Space>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {isExpanded ? 'Collapse' : 'Expand'}
          </Text>
          <MenuOutlined 
            style={{ 
              transform: isExpanded ? 'rotate(90deg)' : 'rotate(0deg)',
              transition: 'transform 0.2s ease'
            }} 
          />
        </Space>
      </Button>

      {/* Expanded Content */}
      {isExpanded && (
        <Card 
          size="small"
          style={{ 
            border: '1px solid #d9d9d9',
            borderRadius: '6px'
          }}
          bodyStyle={{ padding: '16px' }}
        >
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              Drag to reorder • Toggle to show/hide sections
            </Text>
            <Tooltip title="Reset to default order and visibility">
              <Button 
                type="text" 
                icon={<ReloadOutlined />} 
                size="small"
                onClick={resetToDefault}
              />
            </Tooltip>
          </div>
          
          <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
            {sectionOrder.map((sectionKey, index) => {
              const isVisible = sectionVisibility[sectionKey];
              return (
                <div
                  key={sectionKey}
                  className="section-item"
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '12px',
                    padding: '12px 16px',
                    border: '1px solid #f0f0f0',
                    borderRadius: '6px',
                    backgroundColor: isVisible ? '#ffffff' : '#fafafa',
                    opacity: isVisible ? 1 : 0.6,
                    transition: 'all 0.2s ease'
                  }}
                >
                  {/* Position Number */}
                  <div 
                    className="position-number"
                    style={{
                      width: '28px',
                      height: '28px',
                      borderRadius: '50%',
                      backgroundColor: '#1890ff',
                      color: 'white',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '13px',
                      fontWeight: 'bold',
                      flexShrink: 0
                    }}
                  >
                    {index + 1}
                  </div>

                  {/* Section Info */}
                  <div style={{ display: 'flex', alignItems: 'center', flex: 1, gap: '10px' }}>
                    <span className="section-icon" style={{ fontSize: '18px' }}>
                      {SECTION_ICONS[sectionKey as keyof typeof SECTION_ICONS]}
                    </span>
                    <div>
                      <Text strong className="section-title" style={{ fontSize: '14px' }}>
                        {SECTION_LABELS[sectionKey as keyof typeof SECTION_LABELS]}
                      </Text>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '6px', marginTop: '2px' }}>
                        {isVisible ? (
                          <EyeOutlined style={{ fontSize: '11px', color: '#52c41a' }} />
                        ) : (
                          <EyeInvisibleOutlined style={{ fontSize: '11px', color: '#8c8c8c' }} />
                        )}
                        <Text 
                          type="secondary" 
                          className="section-status"
                          style={{ fontSize: '11px' }}
                        >
                          {isVisible ? 'Visible' : 'Hidden'}
                        </Text>
                      </div>
                    </div>
                  </div>

                  {/* Visibility Toggle */}
                  <Tooltip title={`${isVisible ? 'Hide' : 'Show'} section`}>
                    <Switch
                      size="small"
                      checked={isVisible}
                      onChange={() => toggleVisibility(sectionKey)}
                      style={{ flexShrink: 0 }}
                    />
                  </Tooltip>

                  {/* Move Buttons */}
                  <Space size={4}>
                    <Tooltip title="Move up">
                      <Button
                        type="text"
                        icon={<ArrowUpOutlined />}
                        size="small"
                        disabled={index === 0}
                        onClick={() => moveUp(index)}
                        style={{ width: '28px', height: '28px' }}
                      />
                    </Tooltip>
                    <Tooltip title="Move down">
                      <Button
                        type="text"
                        icon={<ArrowDownOutlined />}
                        size="small"
                        disabled={index === sectionOrder.length - 1}
                        onClick={() => moveDown(index)}
                        style={{ width: '28px', height: '28px' }}
                      />
                    </Tooltip>
                  </Space>
                </div>
              );
            })}
          </div>
          
          <Text 
            type="secondary" 
            style={{ 
              display: 'block', 
              marginTop: '16px', 
              fontSize: '11px',
              textAlign: 'center'
            }}
          >
            💡 Use switches to toggle visibility • Use arrows to reorder sections
          </Text>
        </Card>
      )}
    </div>
  );
};

export default SectionOrderControl; 