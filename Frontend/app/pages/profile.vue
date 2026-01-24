<script lang="ts" setup>
import { $authedFetch, handleResponseError, type ApiResponse } from '~/apis/api'
import ProfileForm from '~/components/profile/ProfileForm.vue'
import type { ProfileFormSchema } from '~/components/profile/ProfileForm.vue'

export interface UserProfile {
  id: number
  fullName: string
  nim: string
  programStudy: string
  faculty: string
  generationYear: string
}

const profile = ref<UserProfile | null>(null)
const pending = ref(false)
const form = useTemplateRef<typeof ProfileForm>('form')
const formLoading = ref(false)
const toast = useToast()

async function fetchProfile() {
  try {
    pending.value = true
    const response = await $authedFetch<ApiResponse<UserProfile>>('/users/profile')
    if (response.data) {
      profile.value = response.data
    } else {
      handleResponseError(response)
    }
  } catch (err) {
    handleResponseError(err)
  } finally {
    pending.value = false
  }
}

function openEditForm() {
  if (profile.value) {
    form.value?.setState({
      fullName: profile.value.fullName,
      nim: profile.value.nim,
      programStudy: profile.value.programStudy,
      faculty: profile.value.faculty,
      generationYear: profile.value.generationYear
    })
  }
  form.value?.open()
}

async function onSubmit(data: ProfileFormSchema) {
  try {
    formLoading.value = true
    await $authedFetch('/users/profile', {
      method: 'PUT',
      body: data
    })

    toast.add({
      title: 'Profile updated successfully',
      color: 'success'
    })

    form.value?.close()
    await fetchProfile()
  } catch (error) {
    handleResponseError(error)
  } finally {
    formLoading.value = false
  }
}

onMounted(() => {
  fetchProfile()
})
</script>

<template>
  <div
    class="max-w-[1200px] mx-auto flex flex-col items-center justify-center gap-4 p-4 h-full"
  >
    <UContainer>
      <div class="flex flex-col space-y-6">
        <UPageHeader
          title="Profile"
          description="Manage your personal information"
        />

        <div
          v-if="pending"
          class="flex items-center justify-center py-12"
        >
          <UIcon
            name="i-heroicons-arrow-path"
            class="animate-spin text-4xl"
          />
        </div>

        <UCard
          v-else-if="profile"
          :ui="{
            body: 'space-y-6'
          }"
        >
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold">
              Personal Information
            </h2>
            <UButton
              icon="i-heroicons-pencil"
              color="primary"
              variant="soft"
              @click="openEditForm"
            >
              Edit Profile
            </UButton>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="space-y-2">
              <p class="text-sm font-medium text-gray-500">
                Full Name
              </p>
              <p class="text-base font-semibold">
                {{ profile.fullName }}
              </p>
            </div>

            <div class="space-y-2">
              <p class="text-sm font-medium text-gray-500">
                NIM (Student ID)
              </p>
              <p class="text-base font-semibold">
                {{ profile.nim }}
              </p>
            </div>

            <div class="space-y-2">
              <p class="text-sm font-medium text-gray-500">
                Program Study
              </p>
              <p class="text-base font-semibold">
                {{ profile.programStudy }}
              </p>
            </div>

            <div class="space-y-2">
              <p class="text-sm font-medium text-gray-500">
                Faculty
              </p>
              <p class="text-base font-semibold">
                {{ profile.faculty }}
              </p>
            </div>

            <div class="space-y-2">
              <p class="text-sm font-medium text-gray-500">
                Generation Year
              </p>
              <p class="text-base font-semibold">
                {{ profile.generationYear }}
              </p>
            </div>
          </div>
        </UCard>

        <ProfileForm
          ref="form"
          :loading="formLoading"
          @submit="onSubmit"
        />
      </div>
    </UContainer>
  </div>
</template>
