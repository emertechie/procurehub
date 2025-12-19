import type { AuthUser } from './types'

const jsonHeaders = {
  'Content-Type': 'application/json',
}

async function assertResponseOk(res: Response, fallback: string) {
  if (res.ok) {
    return
  }

  let message = fallback
  try {
    const data = await res.json()
    if (data?.message && typeof data.message === 'string') {
      message = data.message
    }
  } catch {
    // ignore json parse errors
  }

  throw new Error(message)
}

export async function fetchCurrentUser() {
  const res = await fetch('/api/me', {
    credentials: 'include',
  })

  if (!res.ok) {
    throw new Error('Not authenticated')
  }

  return (await res.json()) as AuthUser
}

export async function loginWithCredentials(email: string, password: string) {
  const res = await fetch('/api/login?useCookies=true', {
    method: 'POST',
    headers: jsonHeaders,
    body: JSON.stringify({ email, password }),
    credentials: 'include',
  })

  await assertResponseOk(res, 'Unable to login. Please check your details.')

  const data = (await res.json().catch(() => null)) as { user?: AuthUser } | null
  if (data?.user) {
    return { id: data.user.id, email: data.user.email }
  }

  return fetchCurrentUser().catch(() => null)
}

export async function registerWithCredentials(email: string, password: string) {
  const res = await fetch('/api/register?useCookies=true', {
    method: 'POST',
    headers: jsonHeaders,
    body: JSON.stringify({ email, password }),
    credentials: 'include',
  })

  await assertResponseOk(
    res,
    'Unable to register. Please check your details.',
  )

  const data = (await res.json().catch(() => null)) as { user?: AuthUser } | null
  if (data?.user) {
    return { id: data.user.id, email: data.user.email }
  }

  return fetchCurrentUser().catch(() => null)
}

export async function logoutFromSession() {
  await fetch('/api/logout', {
    method: 'POST',
    credentials: 'include',
  }).catch(() => {
    // ignore logout errors
  })
}
