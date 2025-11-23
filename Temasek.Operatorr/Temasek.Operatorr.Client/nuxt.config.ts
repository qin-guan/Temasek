// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  ssr: false,

  modules: [
    '@nuxt/eslint',
    '@nuxt/image',
    '@nuxt/scripts',
    '@nuxt/test-utils',
    '@nuxt/ui'
  ],

  nitro: {
    cloudflare: {
      wrangler: {
        keep_vars: true,
        vars: {
          SERVICES__TEMASEK_CALENDARR_HTTPS_0: 'https://temasek-calendarr.from.sg',
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
})
