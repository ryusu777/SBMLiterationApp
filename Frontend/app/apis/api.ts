export function useAuthedFetch<T>(
  request: Parameters<typeof useFetch<T>>[0],
  opts?: Parameters<typeof useFetch<T>>[1]
): ReturnType<typeof useFetch> {
  return useFetch<T>(request, {
    ...opts,
    headers: {
      Authorization: `Bearer ${useAuth().getToken()}`
    },
    onResponseError({ response }) {
      if (response.status === 401) {
        useRouter().push('/')
      }
    }
  })
}

export const useAuth = defineStore('auth', () => {
  const token = ref<string | null>(null)
  const refreshToken = ref<string | null>(null)

  // Load token from localStorage on initialization
  if (import.meta.client) {
    token.value = localStorage.getItem('auth_token')
    refreshToken.value = localStorage.getItem('refresh_token')
  }

  function getToken() {
    return token.value
  }

  function setToken(newToken: string) {
    token.value = newToken
    if (import.meta.client) {
      localStorage.setItem('auth_token', newToken)
    }
  }

  function setRefreshToken(newRefreshToken: string) {
    refreshToken.value = newRefreshToken
    if (import.meta.client) {
      localStorage.setItem('refresh_token', newRefreshToken)
    }
  }

  function clearToken() {
    token.value = null
    refreshToken.value = null
    if (import.meta.client) {
      localStorage.removeItem('auth_token')
      localStorage.removeItem('refresh_token')
    }
  }

  return {
    getToken,
    setToken,
    clearToken,
    setRefreshToken
  }
})
