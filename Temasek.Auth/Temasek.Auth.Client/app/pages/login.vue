<script setup lang="ts">
const p = ref(null)
</script>

<template>
  <UDashboardPanel id="login">
    <template #header>
      <UDashboardNavbar title="Login">
        <template #leading>
          <UDashboardSidebarCollapse />
        </template>
      </UDashboardNavbar>
    </template>

    <template #body>
      <div class="flex justify-center h-full">
        <RedirectToSignIn />
        <UProgress
          v-model="p"
        />
      </div>
    </template>
  </UDashboardPanel>
</template>
