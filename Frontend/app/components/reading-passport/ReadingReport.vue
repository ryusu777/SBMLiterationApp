<script setup lang="ts">
// TODO: add discriminator for display for Dashboard (with image) and non Dashboard (without image)
defineProps <{
  report: {
    imageUrl: string
    insight: string
    timeSpent: string
    readDate: string
    currentPage: number
    totalPage: number
  }
}>()
</script>

<template>
  <!-- TODO: Style and space all of below component to match -->
  <UCard>
    <div class="flex flex-row gap-x-4">
      <img
        :src="report.imageUrl"
        alt="Book Cover"
        class="w-32 h-48 object-cover rounded-md aspect-2/3"
      >
      <div class="flex flex-col">
        <div class="flex flex-row justify-between">
          <UBadge>Insight</UBadge>
          <!-- TODO: Use chevron down icon -->
          <nuxt-icon name="chevron-down" />
        </div>
        <p class="mt-2 text-wrap">
          {{ report.insight }}
        </p>

        <USeparator class="my-4" />
        <div class="flex flex-row justify-between">
          <div class="flex flex-row gap-x-2">
            <div class="flex flex-col">
              <p>Total Spent</p>
              <p>{{ report.timeSpent }}</p>
            </div>
            <USeparator orientation="vertical" />
            <div class="flex flex-col">
              <p>Read Date</p>
              <p>{{ report.readDate }}</p>
            </div>
          </div>
          <div class="flex flex-row gap-x-2">
            <p>Page Progress</p>
            <p>{{ report.currentPage }} / {{ report.totalPage }}</p>
          </div>
        </div>
      </div>
    </div>

    <template #footer />
  </UCard>
</template>
