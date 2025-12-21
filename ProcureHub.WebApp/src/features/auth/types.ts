export type AuthUser = {
  id: string;
  email: string;
  roles: string[];
};

export interface AuthContext {
  loading: boolean;
  user: AuthUser | null;
  isAuthenticated: boolean;
  hasRole: (requiredRole: string) => boolean;
}
