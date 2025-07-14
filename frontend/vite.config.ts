import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: '0.0.0.0',
    port: 5173
  },
  resolve: {
    alias: {
      '@': '/src',
      '@components': '/src/components',
      '@common': '/src/components/common',
      '@ui': '/src/components/ui',
      '@features': '/src/features',
      '@layouts': '/src/layouts',
      '@pages': '/src/pages',
      '@hooks': '/src/hooks',
      '@services': '/src/services',
      '@store': '/src/store',
      '@utils': '/src/utils',
      '@types': '/src/types',
      '@assets': '/src/assets',
      '@styles': '/src/styles'
    }
  }
})
