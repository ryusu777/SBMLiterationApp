<script setup lang="ts">
import ReadingReport from './ReadingReport.vue'

const reports = ref([
  {
    imageUrl: 'https://picsum.photos/seed/report1/400/600',
    insight: 'Explored the symbolism of the green light and its representation of Gatsby\'s dreams and aspirations. The narrative technique creates a compelling mystery.',
    timeSpent: '45 min',
    readDate: '2026-01-18',
    currentPage: 89,
    totalPage: 180
  },
  {
    imageUrl: 'https://picsum.photos/seed/report2/400/600',
    insight: 'Deep dive into machine learning algorithms, particularly focusing on neural networks and their practical applications in modern AI systems.',
    timeSpent: '1h 20min',
    readDate: '2026-01-17',
    currentPage: 32,
    totalPage: 45
  },
  {
    imageUrl: 'https://picsum.photos/seed/report3/400/600',
    insight: 'The dystopian themes in 1984 remain eerily relevant today. Orwell\'s portrayal of surveillance and totalitarianism provides important lessons.',
    timeSpent: '55 min',
    readDate: '2026-01-16',
    currentPage: 125,
    totalPage: 328
  }
])
</script>

<template>
  <div class="flex flex-col gap-y-4">
    <ReadingReport
      v-for="(report, index) in reports"
      :key="index"
      :report="report"
    />
  </div>
</template>
