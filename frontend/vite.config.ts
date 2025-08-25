import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: "0.0.0.0",
    port: 3000,
  },
  resolve: {
    alias: {
      "@": "/src",
      "@components": "/src/components",
      "@common": "/src/components/common",
      "@ui": "/src/components/ui",
      "@features": "/src/features",
      "@layouts": "/src/layouts",
      "@pages": "/src/pages",
      "@hooks": "/src/hooks",
      "@services": "/src/services",
      "@store": "/src/store",
      "@utils": "/src/utils",
      "@types": "/src/types",
      "@assets": "/src/assets",
      "@styles": "/src/styles",
    },
  },
  build: {
    target: "esnext",
    minify: "esbuild",
    sourcemap: true,
    rollupOptions: {
      output: {
        manualChunks: (id) => {
          // Vendor chunks
          if (id.includes("node_modules")) {
            if (id.includes("react") || id.includes("react-dom")) {
              return "react-vendor";
            }
            if (id.includes("antd")) {
              return "antd-vendor";
            }
            if (id.includes("@ant-design/icons")) {
              return "icons-vendor";
            }
            return "vendor";
          }

          // Feature-based chunks
          if (id.includes("/pages/")) {
            const pageName = id.split("/pages/")[1]?.split("/")[0];
            if (pageName) {
              return `page-${pageName.toLowerCase()}`;
            }
          }

          if (id.includes("/components/common/")) {
            return "common-components";
          }

          if (id.includes("/hooks/")) {
            return "hooks";
          }

          if (id.includes("/services/")) {
            return "services";
          }

          if (id.includes("/utils/")) {
            return "utils";
          }
        },
      },
    },
  },
  optimizeDeps: {
    include: ["react", "react-dom", "antd", "@ant-design/icons"],
  },
  experimental: {
    renderBuiltUrl(filename, { hostType }) {
      if (hostType === "js") {
        return { js: `/${filename}` };
      }
      return { relative: true };
    },
  },
});
