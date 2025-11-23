<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'

useHead({
  titleTemplate: title => (title ? `${title} - Facilities` : 'Facilities'),
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
      onSelect() {
        open.value = false
      },
    },
    {
      label: 'Bookings',
      icon: 'i-lucide-calendar',
      exact: true,
      to: '/bookings',
      defaultOpen: true,
      onSelect() {
        open.value = false
      },
      children: [
        {
          label: 'Create',
          icon: 'i-lucide-plus',
          to: '/bookings/create',
          onSelect() {
            open.value = false
          },
        },
        {
          label: 'Preview',
          icon: 'i-lucide-glasses',
          to: '/bookings/preview',
          onSelect() {
            open.value = false
          },
        },
      ],
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
        <template #header="{ collapsed }">
          <span v-if="collapsed">
            3SIB
          </span>
          <span v-else>
            Temasek Facilities
          </span>
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
