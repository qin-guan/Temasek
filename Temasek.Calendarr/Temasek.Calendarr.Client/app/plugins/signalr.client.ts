import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

export default defineNuxtPlugin({
  async setup() {
    const runtimeConfig = useRuntimeConfig()

    const hub = new HubConnectionBuilder()
      .withUrl(runtimeConfig.public.api + '/Hubs/Logger')
      .configureLogging(LogLevel.Debug)
      .build()

    hub.onclose(async () => await hub.start())

    await hub.start()

    return {
      provide: {
        signalr: hub,
      },
    }
  },
})
