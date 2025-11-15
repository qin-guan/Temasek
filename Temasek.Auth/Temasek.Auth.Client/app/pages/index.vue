<script setup lang="ts">
import type { PageCardProps } from '@nuxt/ui'

const runtimeConfig = useRuntimeConfig()

const items: (PageCardProps & { id: string })[] = [
  {
    id: 'calendarr',
    title: 'Calendarr',
    description: 'Calendar sync and de-conflicting service',
    icon: 'i-lucide-calendar',
    to: 'https://temasek-calendarr.from.sg',
    target: '_blank',
  },
  {
    id: 'operatorr',
    title: 'Operatorr',
    description: 'Ops-room management',
    icon: 'i-lucide-siren',
    to: 'https://temasek-operatorr.from.sg',
    target: '_blank',
  },
]

const { user, isLoaded } = useUser()

onMounted(async () => {
  await fetch(runtimeConfig.public.api + '/FormSg/Validate', {
    credentials: 'include',
    headers: {
      Authorization: `Bearer ${await useAuth().getToken.value()}`,
    },
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
      <USkeleton
        v-if="!isLoaded"
        class="h-10 w-full"
      />
      <UAlert
        v-else-if="!user?.publicMetadata['nric']"
        color="warning"
        variant="subtle"
        icon="i-lucide-user"
        title="Verify your account"
        description="Verify your identity using FormSG (SingPass) now to gain access to all features!"
        :actions="[
          {
            label: 'Verify now',
            to: runtimeConfig.public.api + '/FormSg/Validate',
          },
        ]"
      />

      <UPageGrid>
        <UPageCard
          v-for="i in items"
          v-bind="i"
          :key="i.id"
        />
      </UPageGrid>
    </template>
  </UDashboardPanel>
</template>
