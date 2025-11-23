import { dark } from '@clerk/themes'

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/eslint',
    '@nuxt/hints',
    '@nuxt/fonts',
    '@nuxt/image',
    '@nuxt/scripts',
    '@nuxt/test-utils',
    '@nuxt/ui',
    '@clerk/nuxt',
    '@vueuse/nuxt',
  ],
  ssr: false,
  devtools: { enabled: true },

  css: ['~/assets/css/main.css'],

  runtimeConfig: {
    public: {
      api: process.env['services__temasek-facilities__https__0'] || process.env.SERVICES__TEMASEK_FACILITIES_HTTPS_0,
    },
  },
  compatibilityDate: '2025-07-15',

  nitro: {
    cloudflare: {
      wrangler: {
        keep_vars: true,
        observability: {
          logs: {
            enabled: true,
            head_sampling_rate: 1,
            invocation_logs: true,
          },
        },
      },
    },
  },

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
