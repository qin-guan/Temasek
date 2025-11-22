<script setup lang="ts">
import { useMutation } from '@tanstack/vue-query'
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
      <div class="space-y-2" />
    </template>
  </UDashboardPanel>
</template>
