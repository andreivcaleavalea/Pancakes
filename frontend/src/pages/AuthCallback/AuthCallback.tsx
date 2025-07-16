import React, { useEffect } from "react";
import { Spin, message } from "antd";
import { useAuth } from "../../contexts/AuthContext";
import { useRouter } from "../../router/RouterProvider";
import { useRef } from "react";

const AuthCallback: React.FC = () => {
  const { updateSession } = useAuth();
  const { navigate } = useRouter();
  const processedRef = useRef(false);

  useEffect(() => {
    if (processedRef.current) return;

    const handleCallback = async () => {
      try {
        const urlParams = new URLSearchParams(window.location.search);
        const code = urlParams.get("code");
        const state = urlParams.get("state");
        const provider = urlParams.get("provider");
        const error = urlParams.get("error");

        const storedState = sessionStorage.getItem("oauth-state");

        if (error) {
          message.error(`Authentication failed: ${error}`);
          navigate("login");
          return;
        }

        if (!state || state !== storedState) {
          message.error("Invalid state parameter");
          navigate("login");
          return;
        }

        if (!code || !provider) {
          message.error("Missing authorization code or provider");
          navigate("login");
          return;
        }

        // Exchange code for user data via backend
        const userData = await exchangeCodeForUser(code, provider);

        if (userData) {
          const session = {
            user: {
              id: userData.user.id,
              name: userData.user.name,
              email: userData.user.email,
              image: userData.user.image,
              provider: userData.user.provider as
                | "google"
                | "github"
                | "facebook",
            },
            token: userData.token,
            expires: userData.expiresAt,
          };

          updateSession(session);
          message.success(`Welcome ${userData.user.name}!`);

          // Set processedRef to true to prevent future runs
          processedRef.current = true;

          navigate("home");
        } else {
          message.error("Failed to get user data");
          navigate("login");
        }
      } catch (error) {
        console.error("Callback error:", error);
        message.error("Authentication failed");
        navigate("login");
      } finally {
        // Clean up
        sessionStorage.removeItem("oauth-state");
        sessionStorage.removeItem("oauth-provider");
      }
    };

    handleCallback();
  }, [updateSession, navigate]);

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        justifyContent: "center",
        alignItems: "center",
        height: "100vh",
        gap: "16px",
      }}
    >
      <Spin size="large" />
      <span>Processing authentication...</span>
    </div>
  );
};

async function exchangeCodeForUser(code: string, provider: string) {
  try {
    const response = await fetch(
      `${import.meta.env.VITE_API_BASE_URL}/auth/login`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          code,
          provider,
          state: sessionStorage.getItem("oauth-state"),
        }),
      }
    );

    if (!response.ok) {
      const error = await response.json();
      console.error(`${provider} login error:`, {
        status: response.status,
        statusText: response.statusText,
        error,
        requestBody: {
          code,
          provider,
          state: sessionStorage.getItem("oauth-state"),
        },
      });
      return null;
    }

    return await response.json();
  } catch (error) {
    console.error("Error exchanging code:", error);
    return null;
  }
}

export default AuthCallback;
