import { Auth } from "@auth/core";
import Google from "@auth/core/providers/google";
import GitHub from "@auth/core/providers/github";
import Facebook from "@auth/core/providers/facebook";

export const authConfig = {
  providers: [
    Google({
      clientId: import.meta.env.VITE_GOOGLE_CLIENT_ID,
      clientSecret: import.meta.env.AUTH_GOOGLE_SECRET || "temp_secret",
    }),
    GitHub({
      clientId: import.meta.env.VITE_GITHUB_CLIENT_ID,
      clientSecret: import.meta.env.AUTH_GITHUB_SECRET || "temp_secret",
    }),
    Facebook({
      clientId: import.meta.env.VITE_FACEBOOK_APP_ID,
      clientSecret: import.meta.env.AUTH_FACEBOOK_SECRET || "temp_secret",
    }),
  ],
  secret:
    import.meta.env.AUTH_SECRET || "your-secret-key-minimum-32-characters",
  trustHost: true,
  callbacks: {
    async signIn({ user, account, profile }) {
      console.log("Sign in callback:", { user, account, profile });
      return true;
    },
    async jwt({ token, user, account }) {
      if (account && user) {
        token.provider = account.provider;
        token.providerAccountId = account.providerAccountId;
      }
      return token;
    },
    async session({ session, token }) {
      if (token) {
        session.user.id = token.sub!;
        session.user.provider = token.provider as string;
        session.user.providerAccountId = token.providerAccountId as string;
      }
      return session;
    },
  },
  pages: {
    signIn: "/login",
    error: "/login",
  },
};
