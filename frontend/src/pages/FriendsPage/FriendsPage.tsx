import React, { useEffect, useState } from "react";
import {
  Typography,
  Card,
  Button,
  Row,
  Col,
  Modal,
  List,
  Avatar,
  message,
  Badge,
  Popconfirm,
  Divider,
  Empty,
  Spin,
} from "antd";
import {
  UserOutlined,
  UserAddOutlined,
  CheckOutlined,
  CloseOutlined,
  HeartOutlined,
  DeleteOutlined,
} from "@ant-design/icons";
import { useAuth } from "@/contexts/AuthContext";
import { useRouter } from "@/router/RouterProvider";
import {
  useFriends,
  useFriendRequests,
  useAvailableUsers,
  usePaginatedFriendsPosts,
} from "@/hooks/useFriends";
import { friendshipApi, type AvailableUser } from "@/services/friendshipApi";
import { BlogCard } from "@/components/common/BlogCard";
import Pagination from "@/components/Pagination/Pagination";
import "./FriendsPage.scss";

const { Title, Text } = Typography;

const FriendsPage: React.FC = () => {
  const { isAuthenticated } = useAuth();
  const { navigate } = useRouter();
  const [isAddFriendsModalOpen, setIsAddFriendsModalOpen] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [sendingRequests, setSendingRequests] = useState<Set<string>>(
    new Set()
  );

  // Use the hooks
  const {
    friends,
    loading: friendsLoading,
    refetch: refetchFriends,
  } = useFriends();
  const {
    requests,
    loading: requestsLoading,
    refetch: refetchRequests,
  } = useFriendRequests();
  const {
    users,
    loading: usersLoading,
    refetch: refetchUsers,
  } = useAvailableUsers();
  const {
    data: friendsPosts,
    pagination,
    loading: postsLoading,
    refetch: refetchPosts,
  } = usePaginatedFriendsPosts(currentPage, 6);

  useEffect(() => {
    if (!isAuthenticated) {
      navigate("login");
    }
  }, [isAuthenticated, navigate]);

  const handleAddFriends = () => {
    setIsAddFriendsModalOpen(true);
    refetchUsers();
  };

  const handleSendFriendRequest = async (receiverId: string) => {
    try {
      setSendingRequests((prev) => new Set(prev).add(receiverId));
      await friendshipApi.sendFriendRequest(receiverId);
      message.success("Friend request sent successfully!");
      refetchUsers(); // Refresh to remove the user from available list
    } catch (error) {
      console.error("Error sending friend request:", error);
      message.error("Failed to send friend request");
    } finally {
      setSendingRequests((prev) => {
        const newSet = new Set(prev);
        newSet.delete(receiverId);
        return newSet;
      });
    }
  };

  const handleAcceptFriendRequest = async (requestId: string) => {
    try {
      await friendshipApi.acceptFriendRequest(requestId);
      message.success("Friend request accepted!");
      refetchRequests();
      refetchFriends();
      refetchPosts();
    } catch (error) {
      console.error("Error accepting friend request:", error);
      message.error("Failed to accept friend request");
    }
  };

  const handleRejectFriendRequest = async (requestId: string) => {
    try {
      await friendshipApi.rejectFriendRequest(requestId);
      message.success("Friend request rejected");
      refetchRequests();
    } catch (error) {
      console.error("Error rejecting friend request:", error);
      message.error("Failed to reject friend request");
    }
  };

  const handleRemoveFriend = async (friendUserId: string) => {
    try {
      await friendshipApi.removeFriend(friendUserId);
      message.success("Friend removed successfully");
      refetchFriends();
      refetchPosts();
    } catch (error) {
      console.error("Error removing friend:", error);
      message.error("Failed to remove friend");
    }
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  if (!isAuthenticated) {
    return null;
  }

  return (
    <div className="friends-page">
      <div className="friends-page__header">
        <Title level={2}>
          <UserOutlined /> Friends (Slices)
        </Title>
        <Text type="secondary">
          Connect with other bloggers and see their latest posts
        </Text>
      </div>

      <Row gutter={[24, 24]}>
        <Col xs={24} lg={12}>
          <Card
            title={`Your Friends (${friends.length})`}
            extra={
              <Button
                type="primary"
                icon={<UserAddOutlined />}
                onClick={handleAddFriends}
              >
                Add Friends
              </Button>
            }
            className="friends-page__card"
            loading={friendsLoading}
          >
            {friends.length === 0 ? (
              <div className="friends-page__empty">
                <Empty
                  description="You don't have any friends yet. Start by adding some friends!"
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                />
              </div>
            ) : (
              <List
                dataSource={friends}
                renderItem={(friend) => (
                  <List.Item
                    actions={[
                      <Popconfirm
                        title="Remove friend"
                        description="Are you sure you want to remove this friend?"
                        onConfirm={() => handleRemoveFriend(friend.userId)}
                        okText="Yes"
                        cancelText="No"
                      >
                        <Button
                          type="text"
                          danger
                          icon={<DeleteOutlined />}
                          size="small"
                        />
                      </Popconfirm>,
                    ]}
                  >
                    <List.Item.Meta
                      avatar={
                        <Avatar
                          src={friend.image}
                          icon={<UserOutlined />}
                          size={48}
                        />
                      }
                      title={friend.name}
                      description={`Friends since ${new Date(
                        friend.friendsSince
                      ).toLocaleDateString()}`}
                    />
                  </List.Item>
                )}
              />
            )}
          </Card>
        </Col>

        <Col xs={24} lg={12}>
          <Card
            title={
              <span>
                Friend Requests
                {requests.length > 0 && (
                  <Badge count={requests.length} style={{ marginLeft: 8 }} />
                )}
              </span>
            }
            className="friends-page__card"
            loading={requestsLoading}
          >
            {requests.length === 0 ? (
              <div className="friends-page__empty">
                <Empty
                  description="No pending friend requests"
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                />
              </div>
            ) : (
              <List
                dataSource={requests}
                renderItem={(request) => (
                  <List.Item
                    actions={[
                      <Button
                        type="primary"
                        icon={<CheckOutlined />}
                        size="small"
                        onClick={() => handleAcceptFriendRequest(request.id)}
                      >
                        Accept
                      </Button>,
                      <Button
                        danger
                        icon={<CloseOutlined />}
                        size="small"
                        onClick={() => handleRejectFriendRequest(request.id)}
                      >
                        Reject
                      </Button>,
                    ]}
                  >
                    <List.Item.Meta
                      avatar={
                        <Avatar
                          src={request.senderImage}
                          icon={<UserOutlined />}
                          size={48}
                        />
                      }
                      title={request.senderName}
                      description={`Sent ${new Date(
                        request.createdAt
                      ).toLocaleDateString()}`}
                    />
                  </List.Item>
                )}
              />
            )}
          </Card>
        </Col>
      </Row>

      <Row gutter={[24, 24]} style={{ marginTop: 24 }}>
        <Col span={24}>
          <Card
            title={
              <span>
                <HeartOutlined style={{ marginRight: 8 }} />
                Friends' Latest Posts
              </span>
            }
            className="friends-page__card"
          >
            {postsLoading ? (
              <div style={{ textAlign: "center", padding: "48px 0" }}>
                <Spin size="large" />
              </div>
            ) : friendsPosts.length === 0 ? (
              <div className="friends-page__empty">
                <Empty
                  description="Your friends haven't posted anything yet. Encourage them to start blogging!"
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                />
              </div>
            ) : (
              <>
                <Row gutter={[16, 16]}>
                  {friendsPosts.map((post) => (
                    <Col xs={24} sm={12} lg={8} key={post.id}>
                      <BlogCard post={post} />
                    </Col>
                  ))}
                </Row>

                {pagination && pagination.totalPages > 1 && (
                  <div className="friends-page__pagination">
                    <Pagination
                      currentPage={pagination.currentPage}
                      totalPages={pagination.totalPages}
                      onPageChange={handlePageChange}
                    />
                  </div>
                )}
              </>
            )}
          </Card>
        </Col>
      </Row>

      {/* Add Friends Modal */}
      <Modal
        title="Add Friends"
        open={isAddFriendsModalOpen}
        onCancel={() => setIsAddFriendsModalOpen(false)}
        footer={null}
        width={600}
        className="friends-page__add-friend-modal"
      >
        {usersLoading ? (
          <div style={{ textAlign: "center", padding: "48px 0" }}>
            <Spin size="large" />
          </div>
        ) : users.length === 0 ? (
          <Empty
            description="No more users available to add as friends"
            image={Empty.PRESENTED_IMAGE_SIMPLE}
          />
        ) : (
          <List
            dataSource={users}
            renderItem={(user: AvailableUser) => (
              <List.Item
                actions={[
                  <Button
                    type="primary"
                    icon={<UserAddOutlined />}
                    loading={sendingRequests.has(user.id)}
                    onClick={() => handleSendFriendRequest(user.id)}
                  >
                    Send Request
                  </Button>,
                ]}
              >
                <List.Item.Meta
                  avatar={
                    <Avatar
                      src={user.image}
                      icon={<UserOutlined />}
                      size={48}
                    />
                  }
                  title={user.name}
                  description={
                    <div>
                      <div>{user.email}</div>
                      {user.bio && (
                        <div
                          style={{
                            marginTop: 4,
                            fontSize: "12px",
                            color: "#888",
                          }}
                        >
                          {user.bio}
                        </div>
                      )}
                    </div>
                  }
                />
              </List.Item>
            )}
          />
        )}
      </Modal>
    </div>
  );
};

export default FriendsPage;
