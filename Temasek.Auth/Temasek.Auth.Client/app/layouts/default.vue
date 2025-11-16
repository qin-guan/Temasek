<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'

useHead({
  titleTemplate: title => (title ? `${title} - Temasek` : 'Temasek'),
})

const { isLoaded, isSignedIn } = useAuth()

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
          Temasek
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

      <UDashboardPanel
        v-if="!isLoaded"
        id="layout-default"
      >
        <template #header>
          <UDashboardNavbar>
            <template #leading>
              <UDashboardSidebarCollapse />
            </template>
          </UDashboardNavbar>
        </template>
        <template #body>
          <UProgress />
        </template>
      </UDashboardPanel>

      <RedirectToSignIn v-else-if="!isSignedIn" />

      <slot v-else />
    </UDashboardGroup>
  </UMain>
</template>
