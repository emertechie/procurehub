import { Result, ResultWithError } from "@/lib/api/types";
import { components } from "@/lib/api/schema";

export type AuthUser = {
  id: string;
  email: string;
};

export interface AuthContext {
  loading: boolean;
  user: AuthUser | null;
  isAuthenticated: boolean;
  login: (
    email: string,
    password: string,
  ) => ResultWithError<components["schemas"]["ProblemDetails"] | null>;
  register: (
    email: string,
    password: string,
  ) => ResultWithError<
    components["schemas"]["HttpValidationProblemDetails"] | null
  >;
  logout: () => Result;
}
