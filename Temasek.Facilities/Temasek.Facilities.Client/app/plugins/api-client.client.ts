import { AllowedHostsValidator, BaseBearerTokenAuthenticationProvider, type AccessTokenProvider } from '@microsoft/kiota-abstractions'
import { FetchRequestAdapter } from '@microsoft/kiota-http-fetchlibrary'
import { createApiClient } from '~/api-client/apiClient'

export default defineNuxtPlugin(() => {
  // Do not run on server because using Clerk tokens, which is on client only
  if (import.meta.server) return

  const runtimeConfig = useRuntimeConfig()
  const { getToken } = useAuth()

  const accessTokenProvider: AccessTokenProvider = {
    async getAuthorizationToken() {
      const token = await getToken.value()
      if (!token) {
        throw new Error('Unable to get token from Clerk.')
      }
      return token
    },
    getAllowedHostsValidator() {
      return new AllowedHostsValidator()
    },
  }
  const authProvider = new BaseBearerTokenAuthenticationProvider(accessTokenProvider)
  const adapter = new FetchRequestAdapter(authProvider)
  adapter.baseUrl = runtimeConfig.public.api

  const apiClient = createApiClient(adapter)

  return {
    provide: {
      apiClient,
    },
  }
})
