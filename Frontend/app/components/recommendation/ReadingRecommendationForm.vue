<script setup lang="ts">
import { z } from 'zod'
import type { FormSubmitEvent } from '#ui/types'
import { $authedFetch, handleResponseError } from '~/apis/api'
import type { PagingResult } from '~/apis/paging'
import GoogleBooksSearchModal from './GoogleBooksSearchModal.vue'
import type { GoogleBookVolume } from './GoogleBooksSearchModal.vue'

interface ReadingCategory {
  id: number
  categoryName: string
}

defineProps<{
  loading?: boolean
}>()

const schema = z.object({
  title: z.string().min(1, 'Title is required').max(200, 'Title must be 200 characters or less'),
  isbn: z.string().min(1, 'ISBN is required').max(50, 'ISBN must be 50 characters or less'),
  readingCategory: z.string().min(1, 'Category is required').max(50, 'Category must be 50 characters or less'),
  authors: z.string().min(1, 'Authors is required').max(500, 'Authors must be 500 characters or less'),
  publishYear: z.string().regex(/^\d{4}$/, 'Publish year must be a 4-digit year'),
  page: z.coerce.number().min(1, 'Page count must be at least 1'),
  resourceLink: z.url('Must be a valid URL').optional().or(z.literal('')),
  coverImageUri: z.url().optional()
})

export type ReadingRecommendationSchema = z.output<typeof schema>

const state = reactive({
  title: '',
  isbn: '',
  readingCategory: '',
  customCategory: '',
  authors: '',
  publishYear: '',
  page: 0,
  resourceLink: '',
  coverImageUri: ''
})

const uploading = ref(false)
const toast = useToast()
const googleBooksModal = useTemplateRef<typeof GoogleBooksSearchModal>('googleBooksModal')
const categories = ref<ReadingCategory[]>([])
const categoriesLoading = ref(false)

const categoryOptions = computed(() => [
  ...categories.value.map(cat => ({
    value: cat.categoryName,
    label: cat.categoryName
  })),
  { value: 'Other', label: 'Other (Custom)' }
])

const isCustomCategory = computed(() => state.readingCategory === 'Other')

const id = ref<number | null>(null)

const emit = defineEmits<{
  (e: 'submit', data: { action: 'Create' | 'Update', data: ReadingRecommendationSchema, id: number | null }): void
}>()

const action = ref<'Create' | 'Update'>('Create')
const open = ref(false)

async function fetchCategories() {
  try {
    categoriesLoading.value = true
    const response = await $authedFetch<PagingResult<ReadingCategory>>('/reading-categories', {
      query: {
        page: 1,
        pageSize: 100
      }
    })
    if (response.rows) {
      categories.value = response.rows
    }
  } catch (err) {
    handleResponseError(err)
  } finally {
    categoriesLoading.value = false
  }
}

onMounted(() => {
  fetchCategories()
})

function create() {
  action.value = 'Create'
  id.value = null
  state.title = ''
  state.isbn = ''
  state.readingCategory = ''
  state.customCategory = ''
  state.authors = ''
  state.publishYear = ''
  state.page = 0
  state.resourceLink = ''
  state.coverImageUri = ''
  open.value = true
}

function update(param: ReadingRecommendationSchema & { id: number }) {
  action.value = 'Update'
  id.value = param.id
  state.title = param.title
  state.isbn = param.isbn

  // Handle category selection
  const categoryExists = categories.value.some(cat => cat.categoryName === param.readingCategory)
  if (categoryExists) {
    state.readingCategory = param.readingCategory
    state.customCategory = ''
  } else {
    state.readingCategory = 'Other'
    state.customCategory = param.readingCategory
  }

  state.authors = param.authors
  state.publishYear = param.publishYear
  state.page = param.page
  state.resourceLink = param.resourceLink || ''
  state.coverImageUri = param.coverImageUri || ''
  open.value = true
}

defineExpose({
  create,
  update,
  close: () => {
    open.value = false
  }
})

async function handleFileUpload(files: File[]) {
  if (!files || !files[0] || files.length === 0) return

  const file = files[0]
  const formData = new FormData()
  formData.append('file', file)

  try {
    uploading.value = true
    const response = await $authedFetch<{
      message: string
      data: {
        url: string
        fileName: string
        fileSize: number
        contentType: string
      }
      errorCode?: string
      errorDescription?: string
      errors?: string[]
    }>('/files/upload', {
      method: 'POST',
      body: formData
    })

    if (response.data?.url) {
      state.coverImageUri = response.data.url
      toast.add({
        title: 'Image uploaded successfully',
        color: 'success'
      })
    }
  } catch (error) {
    handleResponseError(error)
  } finally {
    uploading.value = false
  }
}

async function onSubmit(event: FormSubmitEvent<ReadingRecommendationSchema>) {
  const data = {
    ...event.data,
    readingCategory: isCustomCategory.value ? state.customCategory : event.data.readingCategory
  }
  emit('submit', { action: action.value, data, id: id.value })
}

function openGoogleBooksSearch() {
  googleBooksModal.value?.open()
}

function handleBookSelection(book: GoogleBookVolume) {
  const isbn = book.volumeInfo.industryIdentifiers?.find(
    id => id.type === 'ISBN_13' || id.type === 'ISBN_10'
  )?.identifier || ''

  state.title = book.volumeInfo.title || ''
  state.isbn = isbn

  // Handle category selection
  const bookCategory = book.volumeInfo.categories?.[0] || ''
  const categoryExists = categories.value.some(cat => cat.categoryName === bookCategory)
  if (categoryExists && bookCategory) {
    state.readingCategory = bookCategory
    state.customCategory = ''
  } else if (bookCategory) {
    state.readingCategory = 'Other'
    state.customCategory = bookCategory
  }

  state.authors = book.volumeInfo.authors?.join(', ') || ''
  state.publishYear = book.volumeInfo.publishedDate?.substring(0, 4) || ''
  state.page = book.volumeInfo.pageCount || 0
  state.resourceLink = book.volumeInfo.previewLink || book.volumeInfo.infoLink || ''
  state.coverImageUri = book.volumeInfo.imageLinks?.thumbnail || book.volumeInfo.imageLinks?.smallThumbnail || ''

  toast.add({
    title: 'Book details imported from Google Books',
    color: 'success'
  })
}
</script>

<template>
  <UModal
    v-model:open="open"
    :title="action + ' Reading Recommendation'"
    :description="`Fill in the form below to ${action.toLowerCase()} reading recommendations`"
    :close="{
      variant: 'subtle'
    }"
  >
    <template #body>
      <div class="mb-4 p-4 bg-gray-50 dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="flex items-center justify-between">
          <div>
            <p class="font-medium text-sm text-gray-900 dark:text-gray-100">
              Import from Google Books
            </p>
            <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
              Search and autofill book details from Google Books API
            </p>
          </div>
          <UButton
            icon="i-heroicons-magnifying-glass"
            @click="openGoogleBooksSearch"
          >
            Search
          </UButton>
        </div>
      </div>

      <UForm
        :schema="schema"
        :state="state"
        class="space-y-4"
        @submit="onSubmit"
      >
        <UFormField
          label="Title"
          name="title"
          required
        >
          <UInput
            v-model="state.title"
            placeholder="Enter book title"
            class="w-full"
          />
        </UFormField>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <UFormField
            label="ISBN"
            name="isbn"
            required
          >
            <UInput
              v-model="state.isbn"
              placeholder="Enter ISBN"
              class="w-full"
            />
          </UFormField>

          <UFormField
            label="Publish Year"
            name="publishYear"
            required
          >
            <UInput
              v-model="state.publishYear"
              placeholder="Enter publish year"
              maxlength="4"
              class="w-full"
            />
          </UFormField>
        </div>

        <UFormField
          label="Authors"
          name="authors"
          required
        >
          <UInput
            v-model="state.authors"
            placeholder="Enter authors (comma separated)"
            class="w-full"
          />
        </UFormField>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <UFormField
            label="Category"
            name="readingCategory"
            required
          >
            <USelectMenu
              :model-value="categoryOptions.find(opt => opt.value === state.readingCategory)"
              :items="categoryOptions"
              :loading="categoriesLoading"
              placeholder="Select category"
              class="w-full"
              @update:model-value="(selected) => state.readingCategory = selected.value"
            />
          </UFormField>

          <UFormField
            label="Page Count"
            name="page"
            required
          >
            <UInput
              v-model="state.page"
              type="number"
              placeholder="Enter page count"
              class="w-full"
            />
          </UFormField>
        </div>

        <UFormField
          v-if="isCustomCategory"
          label="Custom Category"
          name="customCategory"
          required
        >
          <UInput
            v-model="state.customCategory"
            placeholder="Enter custom category name"
            class="w-full"
          />
        </UFormField>

        <UFormField
          label="Resource Link"
          name="resourceLink"
        >
          <UInput
            v-model="state.resourceLink"
            type="url"
            placeholder="https://example.com/resource"
            class="w-full"
          />
        </UFormField>

        <UFormField
          label="Cover Image"
          name="coverImageUri"
          required
        >
          <div class="space-y-3">
            <div class="space-y-2">
              <label class="text-sm font-medium text-gray-700 dark:text-gray-200">
                Upload Image
              </label>
              <UInput
                type="file"
                accept="image/*"
                @change="(e) => handleFileUpload(Array.from((e.target as HTMLInputElement).files || []))"
              />
              <div
                v-if="uploading"
                class="text-sm text-gray-500"
              >
                Uploading...
              </div>
            </div>

            <div class="relative flex items-center gap-3">
              <div class="flex-1 border-t border-gray-200 dark:border-gray-700" />
              <span class="text-xs text-gray-500 uppercase">or</span>
              <div class="flex-1 border-t border-gray-200 dark:border-gray-700" />
            </div>

            <div class="space-y-2">
              <label class="text-sm font-medium text-gray-700 dark:text-gray-200">
                Enter Image URL
              </label>
              <UInput
                v-model="state.coverImageUri"
                type="url"
                placeholder="https://example.com/image.jpg"
                class="w-full"
              />
            </div>

            <div
              v-if="state.coverImageUri"
              class="space-y-2 pt-2"
            >
              <label class="text-sm font-medium text-gray-700 dark:text-gray-200">
                Preview
              </label>
              <div class="flex items-start gap-3">
                <img
                  :src="state.coverImageUri"
                  alt="Book cover preview"
                  class="w-32 aspect-[2/3] object-cover rounded border shadow-sm"
                >
                <div class="flex-1 min-w-0">
                  <p class="text-xs text-gray-500 dark:text-gray-400 break-all font-mono bg-gray-50 dark:bg-gray-800 p-2 rounded">
                    {{ state.coverImageUri }}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </UFormField>

        <div class="flex justify-end pt-4">
          <UButton
            type="submit"
            :loading
          >
            Save
          </UButton>
        </div>
      </UForm>
    </template>
  </UModal>

  <GoogleBooksSearchModal
    ref="googleBooksModal"
    @select="handleBookSelection"
  />
</template>
