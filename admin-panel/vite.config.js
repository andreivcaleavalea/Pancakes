import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react()],
    server: {
        port: 3001,
        host: true,
    },
    resolve: {
        alias: {
            "@": "/src",
            "@components": "/src/components",
            "@layouts": "/src/layouts",
            "@pages": "/src/pages",
            "@hooks": "/src/hooks",
            "@services": "/src/services",
            "@contexts": "/src/contexts",
            "@types": "/src/types",
            "@utils": "/src/utils",
            "@lib": "/src/lib",
            "@constants": "/src/constants",
        },
    },
    define: {
        "process.env": {},
    },
    build: {
        rollupOptions: {
            output: {
                manualChunks: {
                    // Separate Ant Design into its own chunk
                    antd: ["antd"],
                    // Separate React libraries
                    "react-vendor": ["react", "react-dom", "react-router-dom"],
                    // Separate other vendor libraries
                    vendor: ["axios", "react-query"],
                    // Separate icons
                    icons: ["@ant-design/icons"],
                },
            },
        },
        // Increase chunk size warning limit to 600kb
        chunkSizeWarningLimit: 600,
    },
});
