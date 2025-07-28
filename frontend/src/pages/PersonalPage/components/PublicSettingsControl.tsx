import React from 'react';
import { Card, Switch, Input, Typography, Space, Button, message } from 'antd';
import { LinkOutlined, EyeOutlined, EyeInvisibleOutlined } from '@ant-design/icons';

const { Text } = Typography;

interface PublicSettingsControlProps {
  isPublic: boolean;
  pageSlug: string;
  onPublicToggle: (isPublic: boolean) => void;
  onPageSlugChange: (slug: string) => void;
}

const PublicSettingsControl: React.FC<PublicSettingsControlProps> = ({
  isPublic,
  pageSlug,
  onPublicToggle,
  onPageSlugChange
}) => {
  const handleSlugChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newSlug = e.target.value.toLowerCase().replace(/[^a-z0-9-]/g, '');
    onPageSlugChange(newSlug);
  };

  const copyPublicUrl = () => {
    const url = `${window.location.origin}/public/${pageSlug}`;
    navigator.clipboard.writeText(url);
    message.success('Public URL copied to clipboard!');
  };

  return (
    <Card 
      size="small"
      className="public-settings-control"
      style={{ 
        marginBottom: '16px',
        border: '1px solid #d9d9d9',
        borderRadius: '6px'
      }}
      bodyStyle={{ padding: '16px' }}
    >
      <div className="public-toggle-section">
        <Space direction="vertical" style={{ width: '100%' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Space>
              {isPublic ? <EyeOutlined /> : <EyeInvisibleOutlined />}
              <Text strong>Public Access</Text>
            </Space>
            <Switch
              checked={isPublic}
              onChange={onPublicToggle}
              checkedChildren="Public"
              unCheckedChildren="Private"
            />
          </div>
          
          {isPublic && (
            <div style={{ marginTop: '12px' }}>
              <Text type="secondary" style={{ fontSize: '12px', display: 'block', marginBottom: '8px' }}>
                Public URL slug:
              </Text>
              <Space.Compact style={{ width: '100%' }}>
                <Input
                  prefix={`${window.location.origin}/public/`}
                  value={pageSlug}
                  onChange={handleSlugChange}
                  placeholder="your-name"
                  style={{ flex: 1 }}
                />
                <Button 
                  icon={<LinkOutlined />} 
                  onClick={copyPublicUrl}
                  title="Copy public URL"
                />
              </Space.Compact>
              <Text type="secondary" style={{ fontSize: '11px', marginTop: '4px', display: 'block' }}>
                💡 Your page will be accessible at this URL when public
              </Text>
            </div>
          )}
        </Space>
      </div>
    </Card>
  );
};

export default PublicSettingsControl; 