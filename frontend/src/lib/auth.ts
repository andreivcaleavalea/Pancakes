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
  facebook: {
    clientId: import.meta.env.VITE_FACEBOOK_APP_ID,
    authUrl: "https://www.facebook.com/v18.0/dialog/oauth",
    scope: "email,public_profile",
  },
} as const;

export type OAuthProvider = keyof typeof oauthProviders;

/**
 * Initiates OAuth login flow for the specified provider
 */
export function initiateOAuthLogin(provider: OAuthProvider): void {
  const config = oauthProviders[provider];
  if (!config || !config.clientId) {
    throw new Error(`Unsupported provider or missing client ID: ${provider}`);
  }

  const state = generateRandomState();
  // Redirect to backend callback URL, which will then redirect to frontend
  const redirectUri = `${
    import.meta.env.VITE_API_BASE_URL || "http://localhost:5141"
  }/auth/${provider}/callback`;

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

  window.location.href = `${config.authUrl}?${params.toString()}`;
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
