import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { VitePWA } from 'vite-plugin-pwa'
import path from 'node:path'

export const pwaManifest = {
  name: 'RentalForge',
  short_name: 'RentalForge',
  description: 'DVD Rental Management Application',
  start_url: '/',
  display: 'standalone' as const,
  theme_color: '#18181b',
  background_color: '#ffffff',
  icons: [
    { src: '/icons/icon-192x192.svg', sizes: '192x192', type: 'image/svg+xml' },
    { src: '/icons/icon-512x512.svg', sizes: '512x512', type: 'image/svg+xml' },
    {
      src: '/icons/icon-maskable-192x192.svg',
      sizes: '192x192',
      type: 'image/svg+xml',
      purpose: 'maskable',
    },
    {
      src: '/icons/icon-maskable-512x512.svg',
      sizes: '512x512',
      type: 'image/svg+xml',
      purpose: 'maskable',
    },
  ],
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
    VitePWA({
      registerType: 'autoUpdate',
      manifest: pwaManifest,
      workbox: {
        globPatterns: ['**/*.{js,css,html,svg,png,ico,woff,woff2}'],
        navigateFallback: '/index.html',
      },
    }),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
})
