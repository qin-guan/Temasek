<script setup lang="ts">
const messages = ref<string[]>([])

const signalr = useSignalrLogger()

function prepend(value, array) {
  const newArray = array.slice()
  newArray.unshift(value)
  return newArray
}

onMounted(() => {
  signalr.on('ReceiveLog', (...items) => {
    messages.value = prepend(items, messages.value).slice(0, 1000)
  })
})
</script>

<template>
  <UDashboardPanel id="logs">
    <template #header>
      <UDashboardNavbar title="Logs">
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
