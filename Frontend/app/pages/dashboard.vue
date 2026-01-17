<template>
  <div
    class="flex flex-col items-center justify-center gap-4 p-4 h-full"
  >
    <UPageCard class="w-full max-w-md">
      Kaulah
    </UPageCard>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  middleware: 'auth'
})
</script>
