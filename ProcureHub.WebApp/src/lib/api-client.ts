type FetchOptions = {
  method?: string
  body?: unknown
  headers?: HeadersInit
}

export class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

export async function apiClient<T>(
  endpoint: string,
  { method = 'GET', body, headers = {} }: FetchOptions = {},
): Promise<T> {
  const res = await fetch(`/api${endpoint}`, {
    method,
    headers: {
      'Content-Type': 'application/json',
      ...headers,
    },
    body: body ? JSON.stringify(body) : undefined,
    credentials: 'include',
  })

  if (!res.ok) {
    let message = `Request failed with status ${res.status}`
    try {
      const data = await res.json()
      if (data?.message) message = data.message
    } catch {
      // ignore
    }
    throw new ApiError(message, res.status)
  }

  // Handle empty responses
  const text = await res.text()
  return text ? JSON.parse(text) : null
}
