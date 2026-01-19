<script lang="ts" setup>
import ReadingReportList from '~/components/reading-passport/ReadingReportList.vue'

const route = useRoute()

const readingResource = ref({
  id: route.params.slug,
  title: 'The Great Gatsby',
  imageUrl: 'https://images.unsplash.com/photo-1524995997946-a1c2e315a42f',
  totalPages: 180,
  totalReadPages: 45,
  type: 'book' as 'book' | 'journal'
})
</script>

<template>
  <!-- TODO: Adjust spacing on the page -->
  <div class="flex flex-col items-center justify-center gap-4 p-4 h-full">
    <UContainer class="flex flex-col gap-y-4">
      <!-- TODO: Style below card to follow figma design -->
      <UCard
        class="overflow-visible mb-[100px]"
      >
        <UPageHeader
          class="border-none"
          title="The great gatsby"
        />

        <template #footer>
          <div class="flex flex-row justify-between relative overflow-visible">
            <img
              :src="readingResource.imageUrl"
              :alt="`${readingResource.title} Cover`"
              class="h-48 aspect-2/3 rounded-md absolute -top-12"
            >
            <div />
            <p>{{ readingResource.totalReadPages }}/{{ readingResource.totalPages }} <br> Pages read</p>
          </div>
        </template>
      </UCard>
      <!-- TODO: Style below infographic to follow figma design -->
      <div class="flex flex-row gap-x-4">
        <UCard class="flex flex-row gap-x-2">
          <div class="flex flex-row gap-x-2">
            <!-- TODO: Adjust this icon -->
            <nuxt-icon
              name="users"
              class="text-[48px]"
            />
            <div class="flex flex-col items-start">
              <p class="font-semibold">
                Author
              </p>
              <p class="text-sm text-gray-500">
                Eric Evans
              </p>
            </div>
          </div>
        </UCard>
        <UCard>
          <div class="flex flex-row gap-x-2">
            <!-- TODO: Adjust this icon -->
            <nuxt-icon
              name="reading"
              class="text-[48px]"
            />
            <div class="flex flex-col items-start">
              <p class="font-semibold">
                Publish Year
              </p>
              <p class="text-sm text-gray-500">
                2003
              </p>
            </div>
          </div>
        </UCard>
        <UCard>
          <div class="flex flex-row gap-x-2">
            <!-- TODO: Adjust this icon -->
            <nuxt-icon
              name="reading"
              class="text-[48px]"
            />
            <div class="flex flex-col items-start">
              <p class="font-semibold">
                ISBN
              </p>
              <p class="text-sm text-gray-500">
                102973049123
              </p>
            </div>
          </div>
        </UCard>
        <UCard>
          <div class="flex flex-row gap-x-2">
            <!-- TODO: Adjust this icon -->
            <nuxt-icon
              name="reading"
              class="text-[48px]"
            />
            <div class="flex flex-col items-start">
              <p class="font-semibold">
                Category
              </p>
              <p class="text-sm text-gray-500">
                Kaulah
              </p>
            </div>
          </div>
        </UCard>
      </div>

      <!-- TODO: Style the report reading session button -->
      <UButton>
        <nuxt-icon name="reading" />
        Report Reading Session
      </UButton>

      <ReadingReportList />
    </UContainer>
  </div>
</template>
