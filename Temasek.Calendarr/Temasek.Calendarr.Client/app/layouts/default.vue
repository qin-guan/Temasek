<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'

useHead({
  titleTemplate: title => (title ? `${title} - Calendarr` : 'Calendarr'),
})

const open = ref(false)

const links = [
  [
    {
      label: 'Dashboard',
      icon: 'i-lucide-house',
      exact: true,
      to: '/',
      onSelect: () => {
        open.value = false
      },
    },
  ],
] satisfies NavigationMenuItem[][]
</script>

<template>
  <UMain>
    <UDashboardGroup unit="rem">
      <UDashboardSidebar
        id="default"
        v-model:open="open"
        collapsible
        resizable
        class="bg-elevated/25"
        :ui="{ footer: 'lg:border-t lg:border-default' }"
      >
        <template #header>
          Temasek Calendarr
        </template>
        <template #default="{ collapsed }">
          <UNavigationMenu
            :collapsed="collapsed"
            :items="links[0]"
            orientation="vertical"
            tooltip
            popover
          />

          <UNavigationMenu
            :collapsed="collapsed"
            :items="links[1]"
            orientation="vertical"
            tooltip
            class="mt-auto"
          />
        </template>

        <template #footer>
          <SignedIn>
            <UserButton />
          </SignedIn>
        </template>
      </UDashboardSidebar>

      <slot />
    </UDashboardGroup>
  </UMain>
</template>
