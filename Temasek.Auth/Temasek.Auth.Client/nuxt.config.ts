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
  ssr: false,
  devtools: { enabled: true },

  css: ['~/assets/css/main.css'],

  runtimeConfig: {
    public: {
      api: process.env['services__temasek-auth__https__0'] || process.env.SERVICES__TEMASEK_AUTH_HTTPS_0,
    },
  },

  compatibilityDate: '2025-07-15',

  nitro: {
    cloudflare: {
      wrangler: {
        keep_vars: true,
        vars: {
          SERVICES__TEMASEK_AUTH_HTTPS_0: 'https://temasek-auth.from.sg',
          PNPM_VERSION: '10.18.3',
        },
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
