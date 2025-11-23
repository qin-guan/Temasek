<script setup lang="ts">
useSeoMeta({
  title: 'Create',
})
</script>

<template>
  <UDashboardPanel id="bookings-create">
    <template #header>
      <UDashboardNavbar title="Create booking">
        <template #leading>
          <UDashboardSidebarCollapse />
        </template>
      </UDashboardNavbar>
    </template>
    <template #body />
  </UDashboardPanel>
</template>
