import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

export const useSignalrLogger = createGlobalState(() => {
  const runtimeConfig = useRuntimeConfig()

  const hub = new HubConnectionBuilder()
    .withUrl(runtimeConfig.public.api + '/Hubs/Logger')
    .configureLogging(LogLevel.Debug)
    .build()

  onMounted(async () => {
    await hub.start()
  })

  onBeforeUnmount(async () => {
    await hub.stop()
  })

  return hub
})

export const useSignalrSync = createGlobalState(() => {
  const runtimeConfig = useRuntimeConfig()

  const hub = new HubConnectionBuilder()
    .withUrl(runtimeConfig.public.api + '/Hubs/Sync')
    .configureLogging(LogLevel.Debug)
    .build()

  onMounted(async () => {
    await hub.start()
  })

  onBeforeUnmount(async () => {
    await hub.stop()
  })

  return hub
})
