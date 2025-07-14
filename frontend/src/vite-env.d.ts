/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_GOOGLE_CLIENT_ID: string;
  readonly VITE_GITHUB_CLIENT_ID: string;
  readonly VITE_FACEBOOK_APP_ID: string;
  readonly AUTH_SECRET: string;
  readonly AUTH_GOOGLE_ID: string;
  readonly AUTH_GOOGLE_SECRET: string;
  readonly AUTH_GITHUB_ID: string;
  readonly AUTH_GITHUB_SECRET: string;
  readonly AUTH_FACEBOOK_ID: string;
  readonly AUTH_FACEBOOK_SECRET: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
