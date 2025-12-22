export type AuthUser = {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
};

export interface AuthContext {
  loading: boolean;
  user: AuthUser | null;
  isAuthenticated: boolean;
  hasRole: (requiredRole: string) => boolean;
}
