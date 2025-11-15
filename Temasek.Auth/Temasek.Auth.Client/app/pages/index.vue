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
  {
    id: 'nameplates',
    title: 'Nameplates',
    description: 'Nameplate generator',
    icon: 'i-lucide-user',
    to: 'https://nameplates.from.sg',
    target: '_blank',
  },
]

const { user, isLoaded } = useUser()
const { getToken } = useAuth()

async function verifyAccount() {
  const data = await $fetch<{ formId: string, prefillFieldId: string, clerkUserId: string }>(runtimeConfig.public.api + '/FormSg/Validate', {
    credentials: 'include',
    headers: {
      Authorization: `Bearer ${await getToken.value()}`,
    },
  })
  const url = `https://form.gov.sg/${data.formId}?${data.prefillFieldId}=${data.clerkUserId}`
  await navigateTo(url, { external: true, open: { target: '_blank' } })
}
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
            onClick: verifyAccount,
          },
        ]"
      />

      <UPageCard
        v-else
        color="success"
        variant="subtle"
        icon="i-lucide-check"
        title="Your account has been verified!"
        description="As a member of the Temasek family, you now have access to all features!"
      >
        <div class="space-y-3">
          <div class="flex flex-wrap gap-3">
            <UFormField label="Name">
              <UInput
                disabled
                :value="user.publicMetadata['name']"
              />
            </UFormField>

            <UFormField label="NRIC">
              <UInput
                disabled
                :value="user.publicMetadata['nric']"
              />
            </UFormField>
          </div>

          <div class="flex gap-3 flex-wrap items-center">
            <UButton
              variant="subtle"
              @click="verifyAccount"
            >
              Re-verify account
            </UButton>
            <p class="text-muted text-sm">
              Re-verify your status at any time!
            </p>
          </div>
        </div>
      </UPageCard>

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
