import * as React from "react";
import { AlertCircle } from "lucide-react";
import { Alert, AlertDescription } from "@/components/ui/alert";

interface FormErrorAlertProps {
  message: string | null;
  className?: string;
}

/**
 * Inline alert for displaying form/mutation errors.
 * Use inside dialogs and forms to show ProblemDetails summary messages.
 */
export function FormErrorAlert({ message, className }: FormErrorAlertProps) {
  if (!message) return null;

  return (
    <Alert variant="destructive" className={className}>
      <AlertCircle className="h-4 w-4" />
      <AlertDescription>{message}</AlertDescription>
    </Alert>
  );
}
