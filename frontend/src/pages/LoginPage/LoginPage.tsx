import React, { useState, useEffect } from "react";
import { Typography, Button, Input, Form, Divider } from "antd";
import { UserOutlined, LockOutlined, MailOutlined } from "@ant-design/icons";
import "./LoginPage.scss";

const { Title, Text } = Typography;

interface LoginFormData {
  email: string;
  password: string;
}

interface RegisterFormData extends LoginFormData {
  name: string;
  confirmPassword: string;
}

interface LoginPageProps {
  initialMode?: "signin" | "register";
}

const LoginPage: React.FC<LoginPageProps> = ({ initialMode = "signin" }) => {
  const [isLoginMode, setIsLoginMode] = useState(initialMode === "signin");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setIsLoginMode(initialMode === "signin");
  }, [initialMode]);

  const handleLogin = async (values: LoginFormData) => {
    setLoading(true);
    console.log("Login:", values);
    // Add login logic here later
    setTimeout(() => setLoading(false), 1000);
  };

  const handleRegister = async (values: RegisterFormData) => {
    setLoading(true);
    console.log("Register:", values);
    // Add register logic here later
    setTimeout(() => setLoading(false), 1000);
  };

  const toggleMode = () => {
    setIsLoginMode(!isLoginMode);
  };

  return (
    <div className="login-page">
      <div className="login-page__container">
        <div className="login-page__header">
          <div className="login-page__logo">
            <div className="login-page__logo-icon">🥞</div>
            <span className="login-page__logo-text">Pancakes</span>
          </div>
          <Title level={2} className="login-page__title">
            {isLoginMode ? "Welcome back" : "Create your account"}
          </Title>
          <Text className="login-page__subtitle">
            {isLoginMode
              ? "Sign in to your account to continue"
              : "Join our community and start sharing your stories"}
          </Text>
        </div>

        <div className="login-page__form-container">
          {isLoginMode ? (
            <Form
              name="login"
              className="login-page__form"
              onFinish={handleLogin}
              layout="vertical"
              size="large"
            >
              <Form.Item
                name="email"
                rules={[
                  { required: true, message: "Please enter your email" },
                  { type: "email", message: "Please enter a valid email" },
                ]}
              >
                <Input
                  prefix={<MailOutlined />}
                  placeholder="Email address"
                  className="login-page__input"
                />
              </Form.Item>

              <Form.Item
                name="password"
                rules={[
                  { required: true, message: "Please enter your password" },
                ]}
              >
                <Input.Password
                  prefix={<LockOutlined />}
                  placeholder="Password"
                  className="login-page__input"
                />
              </Form.Item>

              <Form.Item>
                <Button
                  type="primary"
                  htmlType="submit"
                  className="login-page__submit-btn"
                  loading={loading}
                  block
                >
                  Sign in
                </Button>
              </Form.Item>
            </Form>
          ) : (
            <Form
              name="register"
              className="login-page__form"
              onFinish={handleRegister}
              layout="vertical"
              size="large"
            >
              <Form.Item
                name="name"
                rules={[{ required: true, message: "Please enter your name" }]}
              >
                <Input
                  prefix={<UserOutlined />}
                  placeholder="Full name"
                  className="login-page__input"
                />
              </Form.Item>

              <Form.Item
                name="email"
                rules={[
                  { required: true, message: "Please enter your email" },
                  { type: "email", message: "Please enter a valid email" },
                ]}
              >
                <Input
                  prefix={<MailOutlined />}
                  placeholder="Email address"
                  className="login-page__input"
                />
              </Form.Item>

              <Form.Item
                name="password"
                rules={[
                  { required: true, message: "Please enter your password" },
                  { min: 6, message: "Password must be at least 6 characters" },
                ]}
              >
                <Input.Password
                  prefix={<LockOutlined />}
                  placeholder="Password"
                  className="login-page__input"
                />
              </Form.Item>

              <Form.Item
                name="confirmPassword"
                dependencies={["password"]}
                rules={[
                  { required: true, message: "Please confirm your password" },
                  ({ getFieldValue }) => ({
                    validator(_, value) {
                      if (!value || getFieldValue("password") === value) {
                        return Promise.resolve();
                      }
                      return Promise.reject(
                        new Error("Passwords do not match")
                      );
                    },
                  }),
                ]}
              >
                <Input.Password
                  prefix={<LockOutlined />}
                  placeholder="Confirm password"
                  className="login-page__input"
                />
              </Form.Item>

              <Form.Item>
                <Button
                  type="primary"
                  htmlType="submit"
                  className="login-page__submit-btn"
                  loading={loading}
                  block
                >
                  Create account
                </Button>
              </Form.Item>
            </Form>
          )}

          <Divider className="login-page__divider">or</Divider>

          <div className="login-page__toggle">
            <Text className="login-page__toggle-text">
              {isLoginMode
                ? "Don't have an account?"
                : "Already have an account?"}
            </Text>
            <Button
              type="link"
              onClick={toggleMode}
              className="login-page__toggle-btn"
            >
              {isLoginMode ? "Sign up" : "Sign in"}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
