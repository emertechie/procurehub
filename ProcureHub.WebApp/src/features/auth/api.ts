import { apiClient } from '@/lib/api-client'
import type { AuthUser } from './types'

export async function fetchCurrentUser(): Promise<AuthUser> {
  return apiClient<AuthUser>('/me')
}

export async function loginWithCredentials(
  email: string,
  password: string,
): Promise<AuthUser | null> {
  const data = await apiClient<{ user?: AuthUser }>('/login?useCookies=true', {
    method: 'POST',
    body: { email, password },
  })

  return data?.user ?? (await fetchCurrentUser().catch(() => null))
}

export async function registerWithCredentials(
  email: string,
  password: string,
): Promise<AuthUser | null> {
  const data = await apiClient<{ user?: AuthUser }>('/register?useCookies=true', {
    method: 'POST',
    body: { email, password },
  })

  return data?.user ?? (await fetchCurrentUser().catch(() => null))
}

export async function logoutFromSession(): Promise<void> {
  await apiClient('/logout', { method: 'POST' }).catch(() => {
    // ignore logout errors
  })
}
