import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

export function useSignalrLogger() {
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
}

export function useSignalrSync() {
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
}
