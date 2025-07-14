import React, { useState, useEffect } from "react";
import { Typography, Button, Divider } from "antd";
import { FaGoogle, FaGithub, FaFacebook } from "react-icons/fa";
import "./LoginPage.scss";

const { Title, Text } = Typography;

interface LoginPageProps {
  initialMode?: "signin" | "register";
}

const LoginPage: React.FC<LoginPageProps> = ({ initialMode = "signin" }) => {
  const [isLoginMode, setIsLoginMode] = useState(initialMode === "signin");
  const [loading, setLoading] = useState<string | null>(null);

  useEffect(() => {
    setIsLoginMode(initialMode === "signin");
  }, [initialMode]);

  const handleSocialLogin = async (provider: string) => {
    setLoading(provider);
    console.log(`${isLoginMode ? "Login" : "Register"} with ${provider}`);
    // Add social login logic here later
    setTimeout(() => setLoading(null), 1000);
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
            {isLoginMode ? "Welcome back" : "Join our community"}
          </Title>
          <Text className="login-page__subtitle">
            {isLoginMode
              ? "Sign in to your account to continue"
              : "Create your account and start sharing your stories"}
          </Text>
        </div>

        <div className="login-page__form-container">
          <div className="login-page__social-buttons">
            <Button
              size="large"
              className="login-page__social-btn login-page__social-btn--google"
              icon={<FaGoogle />}
              onClick={() => handleSocialLogin("Google")}
              loading={loading === "Google"}
              block
            >
              {isLoginMode ? "Continue with Google" : "Sign up with Google"}
            </Button>

            <Button
              size="large"
              className="login-page__social-btn login-page__social-btn--github"
              icon={<FaGithub />}
              onClick={() => handleSocialLogin("GitHub")}
              loading={loading === "GitHub"}
              block
            >
              {isLoginMode ? "Continue with GitHub" : "Sign up with GitHub"}
            </Button>

            <Button
              size="large"
              className="login-page__social-btn login-page__social-btn--facebook"
              icon={<FaFacebook />}
              onClick={() => handleSocialLogin("Facebook")}
              loading={loading === "Facebook"}
              block
            >
              {isLoginMode ? "Continue with Facebook" : "Sign up with Facebook"}
            </Button>
          </div>

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
