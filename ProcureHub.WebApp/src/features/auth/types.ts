export type AuthUser = {
  id: string;
  email: string;
};

export interface AuthContext {
  loading: boolean;
  user: AuthUser | null;
  isAuthenticated: boolean;
}
