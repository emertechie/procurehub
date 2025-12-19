export type AuthUser = {
  id: string
  email: string
}

export interface AuthContext {
  loading: boolean
  user: AuthUser | null
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string) => Promise<void>
  logout: () => Promise<void>
}
