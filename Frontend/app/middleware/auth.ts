import { useAuth } from '~/apis/api'

export default defineNuxtRouteMiddleware(() => {
  const auth = useAuth()
  const token = auth.getToken()

  if (!token) {
    return navigateTo('/')
  }
})
