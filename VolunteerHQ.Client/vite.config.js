import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5218',
        changeOrigin: true,
      },
      '/hubs': {
        target: 'http://localhost:5218',
        changeOrigin: true,
        ws: true,
      }
    }
  }
})
