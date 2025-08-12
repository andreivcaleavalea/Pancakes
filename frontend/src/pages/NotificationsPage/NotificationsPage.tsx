import React, { useEffect, useState } from "react";
import {
  Card,
  List,
  Typography,
  Button,
  Empty,
  Spin,
  Alert,
  Badge,
  Divider,
  Space,
  Popconfirm,
  Tag,
  message,
} from "antd";
import {
  BellOutlined,
  DeleteOutlined,
  CheckOutlined,
  FileTextOutlined,
} from "@ant-design/icons";
import { notificationApi } from "../../services/notificationApi";
import { useNotificationContext } from "../../contexts/NotificationContext";
import { useRouter } from "../../router/RouterProvider";
import type { Notification } from "../../types/notification";
import "./NotificationsPage.scss";

const { Title, Text, Paragraph } = Typography;

const NotificationsPage: React.FC = () => {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const { navigate } = useRouter();
  const { markAsRead, markAllAsRead } = useNotificationContext();

  const loadNotifications = async () => {
    try {
      setLoading(true);
      const data = await notificationApi.getAll(1, 50); // Load more notifications at once
      setNotifications(data);
      setError(null);
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to load notifications"
      );
    } finally {
      setLoading(false);
    }
  };

  const handleMarkAsRead = async (notificationId: string) => {
    try {
      setActionLoading(notificationId);

      // Check if notification is already read to avoid double decrementing
      const notification = notifications.find((n) => n.id === notificationId);
      const wasUnread = notification && !notification.isRead;

      // Use context method which automatically updates the badge count
      await markAsRead(notificationId);

      // Update local state
      setNotifications((prev) =>
        prev.map((n) =>
          n.id === notificationId
            ? { ...n, isRead: true, readAt: new Date().toISOString() }
            : n
        )
      );

      message.success("Notification marked as read");
    } catch (err) {
      message.error("Failed to mark notification as read");
    } finally {
      setActionLoading(null);
    }
  };

  const handleMarkAllAsRead = async () => {
    try {
      setActionLoading("all");

      // Use context method which automatically updates the badge count
      await markAllAsRead();

      // Update local state
      setNotifications((prev) =>
        prev.map((n) => ({
          ...n,
          isRead: true,
          readAt: new Date().toISOString(),
        }))
      );

      message.success("All notifications marked as read");
    } catch (err) {
      message.error("Failed to mark all notifications as read");
    } finally {
      setActionLoading(null);
    }
  };

  const handleDelete = async (notificationId: string) => {
    try {
      setActionLoading(notificationId);
      await notificationApi.delete(notificationId);

      // Remove from local state
      setNotifications((prev) => prev.filter((n) => n.id !== notificationId));

      message.success("Notification deleted");
    } catch (err) {
      message.error("Failed to delete notification");
    } finally {
      setActionLoading(null);
    }
  };

  const handleNotificationClick = (notification: Notification) => {
    // Mark as read if not already read
    if (!notification.isRead) {
      handleMarkAsRead(notification.id);
    }

    // Navigate to blog if blogId is available
    if (notification.blogId) {
      navigate("blog-view", { blogId: notification.blogId });
    }
  };

  const getNotificationIcon = (type: string) => {
    switch (type) {
      case "BLOG_REMOVED":
      case "BLOG_STATUS_CHANGED":
        return <FileTextOutlined style={{ color: "#ff4d4f" }} />;
      default:
        return <BellOutlined style={{ color: "#1890ff" }} />;
    }
  };

  const getSourceTag = (source: string) => {
    switch (source) {
      case "ADMIN_ACTION":
        return <Tag color="red">Admin Action</Tag>;
      case "REPORT_RESOLVED":
        return <Tag color="orange">Report Resolved</Tag>;
      default:
        return <Tag color="blue">{source}</Tag>;
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString() + " " + date.toLocaleTimeString();
  };

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  useEffect(() => {
    loadNotifications();
  }, []);

  if (loading) {
    return (
      <div className="notifications-page__loading">
        <Spin size="large" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="notifications-page__error">
        <Alert
          message="Error Loading Notifications"
          description={error}
          type="error"
          showIcon
        />
      </div>
    );
  }

  return (
    <div className="notifications-page">
      <Card className="notifications-page__header">
        <div className="notifications-page__title-section">
          <Space align="center">
            <BellOutlined style={{ fontSize: "24px", color: "#1890ff" }} />
            <Title level={2} style={{ margin: 0 }}>
              Notifications
            </Title>
            {unreadCount > 0 && (
              <Badge
                count={unreadCount}
                style={{ backgroundColor: "#52c41a" }}
              />
            )}
          </Space>
          {notifications.length > 0 && unreadCount > 0 && (
            <Button
              type="primary"
              onClick={handleMarkAllAsRead}
              loading={actionLoading === "all"}
              icon={<CheckOutlined />}
            >
              Mark All Read
            </Button>
          )}
        </div>

        {notifications.length > 0 && (
          <div className="notifications-page__stats">
            <Text type="secondary">
              {notifications.length} total notifications • {unreadCount} unread
            </Text>
          </div>
        )}
      </Card>

      <Card className="notifications-page__content">
        {notifications.length === 0 ? (
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description="No notifications yet"
          >
            <Text type="secondary">
              You'll see notifications here when your blog posts are moderated
              or when other important events occur.
            </Text>
          </Empty>
        ) : (
          <List
            itemLayout="vertical"
            dataSource={notifications}
            renderItem={(notification) => (
              <List.Item
                key={notification.id}
                className={`notification-item ${
                  !notification.isRead ? "notification-item--unread" : ""
                }`}
                actions={[
                  !notification.isRead && (
                    <Button
                      size="small"
                      icon={<CheckOutlined />}
                      onClick={() => handleMarkAsRead(notification.id)}
                      loading={actionLoading === notification.id}
                    >
                      Mark Read
                    </Button>
                  ),
                  <Popconfirm
                    title="Delete this notification?"
                    description="This action cannot be undone."
                    onConfirm={() => handleDelete(notification.id)}
                    okText="Yes"
                    cancelText="No"
                  >
                    <Button
                      size="small"
                      danger
                      icon={<DeleteOutlined />}
                      loading={actionLoading === notification.id}
                    >
                      Delete
                    </Button>
                  </Popconfirm>,
                ].filter(Boolean)}
              >
                <List.Item.Meta
                  avatar={getNotificationIcon(notification.type)}
                  title={
                    <div className="notification-item__header">
                      <span
                        className="notification-item__title"
                        onClick={() => handleNotificationClick(notification)}
                        style={{
                          cursor: notification.blogId ? "pointer" : "default",
                        }}
                      >
                        {notification.title}
                      </span>
                      {getSourceTag(notification.source)}
                      {!notification.isRead && <Badge status="processing" />}
                    </div>
                  }
                  description={
                    <div className="notification-item__content">
                      <Paragraph className="notification-item__message">
                        {notification.message}
                      </Paragraph>

                      {notification.blogTitle && (
                        <div className="notification-item__blog-info">
                          <Text strong>Blog Post: </Text>
                          <Text>{notification.blogTitle}</Text>
                        </div>
                      )}

                      <div className="notification-item__reason">
                        <Text strong>Reason: </Text>
                        <Text type="secondary">{notification.reason}</Text>
                      </div>

                      <div className="notification-item__timestamp">
                        <Text type="secondary" style={{ fontSize: "12px" }}>
                          {formatDate(notification.createdAt)}
                        </Text>
                        {notification.readAt && (
                          <>
                            <Divider type="vertical" />
                            <Text type="secondary" style={{ fontSize: "12px" }}>
                              Read on {formatDate(notification.readAt)}
                            </Text>
                          </>
                        )}
                      </div>
                    </div>
                  }
                />
              </List.Item>
            )}
          />
        )}
      </Card>
    </div>
  );
};

export default NotificationsPage;
