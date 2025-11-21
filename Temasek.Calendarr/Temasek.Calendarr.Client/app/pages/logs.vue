<script setup lang="ts">
const messages = ref<string[]>([])

const { $signalr } = useNuxtApp()

function prepend(value, array) {
  const newArray = array.slice()
  newArray.unshift(value)
  return newArray
}

onMounted(() => {
  $signalr.on('ReceiveLog', (...items) => {
    messages.value = prepend(items, messages.value)
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
        <UTable :data="messages" />
      </div>
    </template>
  </UDashboardPanel>
</template>
