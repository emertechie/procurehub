import type { components } from "./schema";

export type ProblemDetails =
  components["schemas"]["HttpValidationProblemDetails"];

export class ProblemDetailsError extends Error {
  readonly status: number;
  readonly title: string;
  readonly detail: string | null;
  readonly instance: string | null;
  readonly errors: Record<string, string[]>;

  constructor(problem: ProblemDetails) {
    super(problem.detail ?? problem.title ?? "An error occurred");
    this.name = "ProblemDetailsError";
    this.status = Number(problem.status) || 400;
    this.title = problem.title ?? "Bad Request";
    this.detail = problem.detail ?? null;
    this.instance = problem.instance ?? null;
    this.errors = problem.errors ?? {};
  }

  /** Summary message for inline alerts */
  get summary(): string {
    return this.title ?? this.detail;
  }

  /** Check if there are field-level validation errors */
  get hasFieldErrors(): boolean {
    return Object.keys(this.errors).length > 0;
  }
}

export function isProblemDetailsError(
  error: unknown,
): error is ProblemDetailsError {
  return error instanceof ProblemDetailsError;
}

/** Type guard to check if response body looks like ProblemDetails */
export function isProblemDetails(body: unknown): body is ProblemDetails {
  return (
    typeof body === "object" &&
    body !== null &&
    "status" in body &&
    typeof (body as ProblemDetails).status === "number"
  );
}
