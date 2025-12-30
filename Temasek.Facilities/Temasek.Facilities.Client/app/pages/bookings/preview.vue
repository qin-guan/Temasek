<script setup lang="ts">
import { VueCal, type VueCalConfig } from 'vue-cal'
import 'vue-cal/style'

useSeoMeta({
  title: 'Bookings',
})

const { $apiClient } = useNuxtApp()

onMounted(async () => {
  await $apiClient.facility.get()
})

const colorMode = useColorMode()

const cal = useTemplateRef('cal')

// const facilitiesUnderFacilityType = computed(() => {
//   if (facilitiesIsPending.value) {
//     return []
//   }

//   return facilities.value?.filter(n => n.group === facilityType.value)
// })

// const bookingsUnderFacilityType = computed(() => {
//   if (bookingsIsPending.value) {
//     return []
//   }

//   return bookings.value?.filter(n => facilitiesUnderFacilityType.value?.some(nn => nn.name == n.facilityName))
// })

const calOptions = computed<VueCalConfig>(() => {
  // const events = []

  // for (const booking of bookingsUnderFacilityType.value ?? []) {
  //   events.push({
  //     id: booking?.id,
  //     start: booking.startDateTime,
  //     end: booking.endDateTime,
  //     schedule: facilitiesUnderFacilityType.value?.findIndex(n => n.name === booking.facilityName) + 1,
  //     title: booking?.user?.unit + ' / ' + booking?.conduct,
  //     content: '<br>' + booking?.description + '<br>' + booking?.pocName + '<br>' + booking?.pocPhone,
  //     draggable: false,
  //     resizable: false,
  //     deletable: false,
  //   })
  // }

  return {
    // view: view.value,
    views: ['day', 'week'],
    // viewDate: startDate.value,
    snapToInterval: 30,
    eventCreateMinDrag: 20,
    timeStep: 30,
    editableEvents: false,
    // schedules: facilities.value?.filter(n => n.group === facilityType.value).map(name => ({ label: name.name })),
    // events,
    // onReady,
    // onEventCreate,
    // onEventResizeEnd,
    style: 'flex: 1',
  }
})
</script>

<template>
  <UDashboardPanel id="bookings-preview">
    <template #header>
      <UDashboardNavbar title="Schedule preview">
        <template #leading>
          <UDashboardSidebarCollapse />
        </template>
        <template #right>
          <UTooltip text="Add to Calendar">
            <UButton
              icon="i-lucide-calendar-arrow-down"
              variant="ghost"
            />
          </UTooltip>
        </template>
      </UDashboardNavbar>
    </template>
    <template #body>
      <VueCal
        ref="cal"
        :dark="colorMode.value === 'dark'"
        v-bind="calOptions"
      >
        <template #schedule-heading="{ schedule }">
          <strong>{{ schedule.label }}</strong>
        </template>
      </VueCal>
    </template>
  </UDashboardPanel>
</template>

<style scoped>
.vuecal {
  --vuecal-primary-color: var(--ui-primary);
  --vuecal-height: 100%;
}

:deep(.vuecal__event-placeholder) {
  background-color: var(--ui-primary);
}
</style>
