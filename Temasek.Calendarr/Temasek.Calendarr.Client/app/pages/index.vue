<script setup lang="ts">
import { useMutation } from '@tanstack/vue-query'

const signalr = useSignalrSync()

const forceRefresh = useMutation({
  async mutationFn(_: MouseEvent) {
    await signalr.invoke('RunFullSyncAsync')
    await navigateTo('/logs')
  },
})
</script>

<template>
  <UDashboardPanel id="dashboard">
    <template #header>
      <UDashboardNavbar title="Dashboard">
        <template #leading>
          <UDashboardSidebarCollapse />
        </template>
      </UDashboardNavbar>
    </template>

    <template #body>
      <div class="space-y-2">
        <UButton @click="forceRefresh.mutate">
          Force refresh
        </UButton>
      </div>
    </template>
  </UDashboardPanel>
</template>
