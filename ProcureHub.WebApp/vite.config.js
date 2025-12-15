import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { tanstackRouter } from '@tanstack/router-plugin/vite'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    tanstackRouter({ target: 'react', autoCodeSplitting: true }),
    react(),
  ],
  server: {
    port: parseInt(process.env.VITE_PORT),
    proxy: {
      // "api" is the name of the API in AppHost.cs.
      '/api': {
        target: process.env.services__api__https__0 || process.env.services__api__http__0,
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/api/, '')
      }
    }    
  }
})
