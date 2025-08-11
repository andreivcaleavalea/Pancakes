// OAuth provider configurations for the stateless authentication system
export const oauthProviders = {
  google: {
    clientId: import.meta.env.VITE_GOOGLE_CLIENT_ID,
    authUrl: "https://accounts.google.com/o/oauth2/v2/auth",
    scope: "openid email profile",
  },
  github: {
    clientId: import.meta.env.VITE_GITHUB_CLIENT_ID,
    authUrl: "https://github.com/login/oauth/authorize",
    scope: "user:email",
  },
} as const;

export type OAuthProvider = keyof typeof oauthProviders;

/**
 * Initiates OAuth login flow for the specified provider
 */
export function initiateOAuthLogin(provider: OAuthProvider): void {
  console.log(`🔐 OAuth: Starting login flow for ${provider}`);

  const config = oauthProviders[provider];
  
  if (!config || !config.clientId) {
    console.error(
      `❌ OAuth: Unsupported provider or missing client ID: ${provider}`
    );
    throw new Error(`Unsupported provider or missing client ID: ${provider}`);
  }

  const state = generateRandomState();
  // Redirect to backend callback URL, which will then redirect to frontend
  const redirectUri = `${
    import.meta.env.VITE_USER_API_URL || "http://localhost:5141"
  }/auth/${provider}/callback`;

  console.log(`🔐 OAuth: Generated state and redirect URI:`, {
    provider,
    redirectUri,
    state: state.substring(0, 8) + "...", // Only log first 8 chars for security
    scope: config.scope,
  });

  // Store state for verification
  sessionStorage.setItem("oauth-state", state);
  sessionStorage.setItem("oauth-provider", provider);

  const params = new URLSearchParams({
    client_id: config.clientId,
    redirect_uri: redirectUri,
    scope: config.scope,
    response_type: "code",
    state: state,
  });

  const authUrl = `${config.authUrl}?${params.toString()}`;
  console.log(`🔐 OAuth: Redirecting to ${provider} authorization server...`);

  window.location.href = authUrl;
}

/**
 * Generates a random state for OAuth security
 */
function generateRandomState(): string {
  return (
    Math.random().toString(36).substring(2, 15) +
    Math.random().toString(36).substring(2, 15)
  );
}
