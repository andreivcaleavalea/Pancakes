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
      console.log("🔐 AuthCallback: Processing OAuth callback...");

      try {
        const urlParams = new URLSearchParams(window.location.search);
        const code = urlParams.get("code");
        const state = urlParams.get("state");
        const provider = urlParams.get("provider");
        const error = urlParams.get("error");

        console.log("🔐 AuthCallback: URL parameters:", {
          hasCode: !!code,
          hasState: !!state,
          provider,
          error,
          codeLength: code?.length,
        });

        const storedState = sessionStorage.getItem("oauth-state");
        console.log("🔐 AuthCallback: State verification:", {
          receivedState: state?.substring(0, 8) + "...",
          storedState: storedState?.substring(0, 8) + "...",
          stateMatch: state === storedState,
        });

        if (error) {
          console.error("❌ AuthCallback: OAuth error received:", error);
          message.error(`Authentication failed: ${error}`);
          navigate("login");
          return;
        }

        if (!state || state !== storedState) {
          console.error("❌ AuthCallback: Invalid state parameter");
          message.error("Invalid state parameter");
          navigate("login");
          return;
        }

        if (!code || !provider) {
          console.error(
            "❌ AuthCallback: Missing authorization code or provider"
          );
          message.error("Missing authorization code or provider");
          navigate("login");
          return;
        }

        console.log(
          `🔐 AuthCallback: Exchanging code for user data with ${provider}...`
        );

        // Exchange code for user data via backend
        const userData = await exchangeCodeForUser(code, provider);

        if (userData) {
          console.log("✅ AuthCallback: User data received:", {
            userId: userData.user.id,
            userName: userData.user.name,
            userEmail: userData.user.email,
            provider: userData.user.provider,
            hasToken: !!userData.token,
            expiresAt: userData.expiresAt,
          });

          const session = {
            user: {
              id: userData.user.id,
              name: userData.user.name,
              email: userData.user.email,
              image: userData.user.image,
              provider: userData.user.provider as "google" | "github",
            },
            token: userData.token,
            expires: userData.expiresAt,
          };

          updateSession(session);
          message.success(`Welcome ${userData.user.name}!`);
          console.log(
            "✅ AuthCallback: Authentication successful, redirecting to home"
          );
          navigate("home");
        } else {
          console.error("❌ AuthCallback: Failed to get user data");
          message.error("Failed to get user data");
          navigate("login");
        }
      } catch (error) {
        console.error("Callback error:", error);
        
        // Check if this is a ban error - redirect to banned page
        if (error instanceof Error && (error as any).isBanError) {
          const banData = (error as any).banData;
          // Store ban information for the banned page
          sessionStorage.setItem("ban-info", JSON.stringify({
            message: banData.message,
            timestamp: new Date().toISOString()
          }));
          navigate("banned");
          return;
        }
        
        // Show specific error message if available, otherwise show generic message
        if (error instanceof Error && error.message) {
          message.error(error.message, 5);
        } else {
          message.error("Authentication failed");
        }
        
        navigate("login");
      } finally {
        // Clean up
        console.log("🔐 AuthCallback: Cleaning up session storage");
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
  console.log(
    `🔐 AuthCallback: Making API call to exchange code for ${provider} user data...`
  );

  try {
    const requestBody = {
      code,
      provider,
      state: sessionStorage.getItem("oauth-state"),
    };

    console.log("🔐 AuthCallback: Request details:", {
      url: `${import.meta.env.VITE_API_BASE_URL}/auth/login`,
      provider,
      hasCode: !!code,
      hasState: !!requestBody.state,
      codeLength: code?.length,
    });

    const response = await fetch(
      `${import.meta.env.VITE_API_BASE_URL}/auth/login`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(requestBody),
      }
    );

    console.log(
      `🔐 AuthCallback: API response status: ${response.status} ${response.statusText}`
    );

    if (!response.ok) {
      let errorMessage = "Authentication failed";
      
      try {
        const errorData = await response.json();
        
        // Handle specific error cases with user-friendly messages
        if (response.status === 401 && errorData.message) {
          // Check if this is a ban message (contains "banned" keyword)
          if (errorData.message.toLowerCase().includes('banned')) {
            // This is a ban - return special error type with ban data
            const banError = new Error(errorData.message);
            (banError as any).isBanError = true;
            (banError as any).banData = {
              message: errorData.message,
              status: response.status
            };
            throw banError;
          }
          errorMessage = errorData.message;
        } else if (response.status === 400) {
          errorMessage = "Invalid login request. Please try again.";
        } else if (response.status >= 500) {
          errorMessage = "Server error. Please try again later.";
        } else if (errorData.message) {
          errorMessage = errorData.message;
        }
        
        console.error(`${provider} login error:`, {
          status: response.status,
          statusText: response.statusText,
          error: errorData,
          requestBody: {
            code,
            provider,
            state: sessionStorage.getItem("oauth-state"),
          },
        });
      } catch (parseError) {
        // Only catch JSON parsing errors, not custom ban errors
        if (parseError instanceof Error && (parseError as any).isBanError) {
          // Re-throw ban errors so they can be handled properly
          throw parseError;
        }
        console.error("Failed to parse error response:", parseError);
        errorMessage = `Login failed (${response.status}). Please try again.`;
      }
      
      // Throw the error with the specific message so it can be caught and displayed
      throw new Error(errorMessage);
    }

    const userData = await response.json();
    console.log("✅ AuthCallback: Successfully exchanged code for user data");
    return userData;
  } catch (error) {
    console.error("Error exchanging code:", error);
    // Re-throw to maintain error message
    if (error instanceof Error) {
      throw error;
    }
    throw new Error("Network error. Please check your connection and try again.");
  }
}

export default AuthCallback;
