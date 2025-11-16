export default defineNuxtRouteMiddleware((to) => {
  const { isSignedIn } = useAuth()

  if (isSignedIn.value) {
    if (to.path === '/login') {
      return navigateTo('/')
    }
  }
  else {
    if (to.path !== '/login') {
      return navigateTo('/login')
    }
  }
})
