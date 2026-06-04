/// <reference types="vitest" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Keep the SPA same-origin with the API: proxy /api to the backend so
      // there are no CORS or cookie-SameSite issues in dev, and it mirrors the
      // reverse-proxy setup we'll use in production. Backend dev URL comes from
      // launchSettings.json (http profile).
      '/api': {
        target: 'http://localhost:5148',
        changeOrigin: true,
        secure: false,
      },
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test-setup.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'lcov'],
      reportsDirectory: './coverage',
      include: ['src/**/*.{ts,tsx}'],
      exclude: ['src/**/*.d.ts', 'src/main.tsx', 'src/vite-env.d.ts', 'src/test-setup.ts'],
    },
  },
})
