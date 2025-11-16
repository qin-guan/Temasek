<script setup lang="ts">
const messages = ref<string[]>([])

const { $signalr } = useNuxtApp()

function prepend(value, array) {
  const newArray = array.slice()
  newArray.unshift(value)
  return newArray
}

onMounted(() => {
  $signalr.on('ReceiveLog', (logLevel, category, message) => {
    messages.value = prepend(`[${category}] ${message}`, messages.value)
  })
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
        <p
          v-for="msg in messages"
          :key="msg"
          class="font-mono"
        >
          {{ msg }}
        </p>
      </div>
    </template>
  </UDashboardPanel>
</template>
