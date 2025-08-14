import React, { useState, useEffect } from "react";
import { Layout, Button, Menu, Drawer, Avatar, Dropdown } from "antd";
import { MenuOutlined, UserOutlined, LogoutOutlined } from "@ant-design/icons";
import { useAuth } from "../../contexts/AuthContext";
import { useRouter } from "../../router/RouterProvider";
import { getProfilePictureUrl } from "../../utils/imageUtils";
import SearchDropdown from "../../components/common/SearchDropdown/SearchDropdown";
import { NotificationBadge } from "../../components/common/NotificationBadge";
import { useNotificationContext } from "../../contexts/NotificationContext";
import "./Header.scss";

const { Header: AntHeader } = Layout;

const Header: React.FC = () => {
  const { user, isAuthenticated, signOut } = useAuth();
  const { navigate, currentPage } = useRouter();
  const { unreadCount: notificationCount } = useNotificationContext();
  const [drawerVisible, setDrawerVisible] = useState(false);
  const [showMobileMenu, setShowMobileMenu] = useState(false);

  // Function to get the selected menu key based on current page
  const getSelectedKey = () => {
    switch (currentPage) {
      case "home":
        return ["home"];
      case "saved":
        return ["saved"];
      case "friends":
        return ["friends"];
      case "drafts":
        return ["drafts"];
      case "notifications":
        return ["notifications"];
      case "profile":
        return ["profile"];
      default:
        return [];
    }
  };

  const handleMenuClick = (key: string) => {
    switch (key) {
      case "home":
        navigate("home");
        break;
      case "saved":
        navigate("saved");
        break;
      case "friends":
        navigate("friends");
        break;
      case "drafts":
        navigate("drafts");
        break;
      case "notifications":
        navigate("notifications");
        break;
      case "profile":
        navigate("profile");
        break;
      default:
        break;
    }
  };

  const menuItems = [
    { key: "home", label: "Home" },
    { key: "saved", label: "Saved" },
    { key: "friends", label: "Friends" },
    { key: "drafts", label: "Drafts" },
    {
      key: "notifications",
      label: (
        <span style={{ display: "flex", alignItems: "center" }}>
          Notifications
          {isAuthenticated && notificationCount > 0 && (
            <NotificationBadge count={notificationCount} size="small" />
          )}
        </span>
      ),
    },
    { key: "profile", label: "Profile" },
  ];

  useEffect(() => {
    const handleResize = () => {
      setShowMobileMenu(window.innerWidth < 700);
    };

    handleResize();
    window.addEventListener("resize", handleResize);
    return () => window.removeEventListener("resize", handleResize);
  }, []);

  const handleSignIn = () => {
    navigate("login", "signin");
  };

  const handleRegister = () => {
    navigate("login", "register");
  };

  const handleLogoClick = () => {
    navigate("home");
  };

  const handleLogout = () => {
    signOut();
    navigate("home");
  };

  const userMenuItems = [
    {
      key: "profile",
      label: "Profile",
      icon: <UserOutlined />,
      onClick: () => navigate("profile"),
    },
    {
      key: "logout",
      label: "Logout",
      icon: <LogoutOutlined />,
      onClick: handleLogout,
    },
  ];

  const renderAuthSection = () => {
    if (isAuthenticated && user) {
      return (
        <div className="header__user">
          <Dropdown menu={{ items: userMenuItems }} placement="bottomRight">
            <div className="header__user-profile">
              <Avatar
                src={getProfilePictureUrl(user.image)}
                alt={user.name}
                size="default"
                icon={<UserOutlined />}
              />
              <span className="header__user-name">{user.name}</span>
            </div>
          </Dropdown>
        </div>
      );
    }

    return (
      <div className="header__auth">
        <Button
          className="header__btn header__btn--secondary"
          onClick={handleSignIn}
        >
          Sign in
        </Button>
        <Button
          type="primary"
          className="header__btn header__btn--primary"
          onClick={handleRegister}
        >
          Register
        </Button>
      </div>
    );
  };

  const renderMobileAuthSection = () => {
    if (isAuthenticated && user) {
      return (
        <div className="header__mobile-user">
          <div className="header__user-profile">
            <Avatar
              src={getProfilePictureUrl(user.image)}
              alt={user.name}
              size="default"
              icon={<UserOutlined />}
            />
            <span className="header__user-name">{user.name}</span>
          </div>
          <Button
            block
            className="header__btn header__btn--secondary"
            icon={<UserOutlined />}
            onClick={() => {
              navigate("profile");
              setDrawerVisible(false);
            }}
          >
            Profile
          </Button>
          <Button
            block
            className="header__btn header__btn--primary"
            icon={<LogoutOutlined />}
            onClick={handleLogout}
          >
            Logout
          </Button>
        </div>
      );
    }

    return (
      <div className="header__auth header__auth--mobile">
        <Button
          block
          className="header__btn header__btn--secondary"
          onClick={handleSignIn}
        >
          Sign in
        </Button>
        <Button
          block
          type="primary"
          className="header__btn header__btn--primary"
          onClick={handleRegister}
        >
          Register
        </Button>
      </div>
    );
  };

  return (
    <AntHeader className="header">
      <div className="header__container">
        <div className="header__left">
          <div
            className="header__logo"
            onClick={handleLogoClick}
            style={{ cursor: "pointer" }}
          >
            <div className="header__logo-icon">🥞</div>
            <span className="header__logo-text">Pancakes</span>
          </div>

          {!showMobileMenu && (
            <div className="header__search header__search--desktop">
              <SearchDropdown />
            </div>
          )}
        </div>

        {!showMobileMenu && (
          <div className="header__center">
            <Menu
              mode="horizontal"
              items={menuItems}
              className="header__menu"
              selectedKeys={getSelectedKey()}
              disabledOverflow={true}
              onSelect={({ key }) => handleMenuClick(key)}
            />
          </div>
        )}

        <div className="header__right">
          {!showMobileMenu && renderAuthSection()}

          {showMobileMenu && (
            <Button
              className="header__mobile-menu"
              type="text"
              icon={<MenuOutlined />}
              onClick={() => setDrawerVisible(true)}
            />
          )}
        </div>
      </div>

      <Drawer
        title="Menu"
        placement="right"
        onClose={() => setDrawerVisible(false)}
        open={drawerVisible}
        className="header__drawer"
        width={window.innerWidth < 400 ? "80%" : 280}
      >
        <div className="header__mobile-content">
          <div className="header__search header__search--mobile">
            <SearchDropdown />
          </div>

          <Menu
            mode="vertical"
            items={menuItems}
            className="header__mobile-menu-items"
            selectedKeys={getSelectedKey()}
            onSelect={({ key }) => {
              handleMenuClick(key);
              setDrawerVisible(false); // Close drawer on mobile after selection
            }}
          />

          {renderMobileAuthSection()}
        </div>
      </Drawer>
    </AntHeader>
  );
};

export default Header;
