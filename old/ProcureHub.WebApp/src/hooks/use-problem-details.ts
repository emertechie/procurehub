import { useMemo } from "react";
import type { FieldValues, Path, UseFormReturn } from "react-hook-form";
import {
  isProblemDetailsError,
  type ProblemDetailsError,
} from "@/lib/api/problem-details";

interface ProblemDetailsResult {
  /** The error if it's a ProblemDetailsError, null otherwise */
  error: ProblemDetailsError | null;
  /** Summary message (detail or title) for inline alerts */
  summary: string | null;
  /** Field-level validation errors */
  fieldErrors: Record<string, string[]>;
  /** Whether there are field-level errors */
  hasFieldErrors: boolean;
}

/**
 * Extract ProblemDetails info from a mutation error.
 * Returns null values if error is not a ProblemDetailsError.
 */
export function useProblemDetails(error: unknown): ProblemDetailsResult {
  return useMemo(() => {
    if (!error || !isProblemDetailsError(error)) {
      return {
        error: null,
        summary: null,
        fieldErrors: {},
        hasFieldErrors: false,
      };
    }

    return {
      error,
      summary: error.summary,
      fieldErrors: error.errors,
      hasFieldErrors: error.hasFieldErrors,
    };
  }, [error]);
}

/**
 * Sync ProblemDetails field errors to a react-hook-form instance.
 * Call this in a useEffect when fieldErrors change.
 *
 * @param form - The react-hook-form instance
 * @param fieldErrors - Field errors from useProblemDetails
 */
export function setFormFieldErrors<T extends FieldValues>(
  form: UseFormReturn<T>,
  fieldErrors: Record<string, string[]>,
): void {
  for (const [field, messages] of Object.entries(fieldErrors)) {
    // Convert PascalCase field names from API to camelCase for form fields
    const formField = field.charAt(0).toLowerCase() + field.slice(1);
    if (messages.length > 0) {
      form.setError(formField as Path<T>, {
        type: "server",
        message: messages
          .map((msg) => (msg.endsWith(".") ? msg.slice(0, -1) : msg))
          .join(". "),
      });
    }
  }
}
