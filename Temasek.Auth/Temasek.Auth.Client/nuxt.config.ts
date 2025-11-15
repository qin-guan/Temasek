import { dark } from '@clerk/themes'

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({

  modules: [
    '@nuxt/eslint',
    '@nuxt/image',
    '@nuxt/scripts',
    '@nuxt/test-utils',
    '@nuxt/ui',
    '@nuxt/fonts',
    '@clerk/nuxt',
  ],
  devtools: { enabled: true },

  css: ['~/assets/css/main.css'],

  runtimeConfig: {
    public: {
      api: process.env['services__temasek-auth__https__0'] || process.env.SERVICES__TEMASEK_AUTH_HTTPS_0,
    },
  },

  compatibilityDate: '2025-07-15',

  clerk: {
    appearance: {
      theme: dark,
    },
  },

  eslint: {
    config: {
      stylistic: true,
    },
  },
})
