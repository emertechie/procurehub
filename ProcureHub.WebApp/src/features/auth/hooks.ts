import { useContext } from 'react'
import { redirect } from '@tanstack/react-router'

import { AuthContext } from './store'
import type { AuthContext as AuthContextValue } from './types'

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return ctx
}

export function ensureAuthenticated(
  authContext: AuthContextValue,
  currentHref: string,
) {
  if (!authContext.isAuthenticated) {
    throw redirect({
      to: '/login',
      search: {
        redirect: currentHref,
      },
    })
  }
}
