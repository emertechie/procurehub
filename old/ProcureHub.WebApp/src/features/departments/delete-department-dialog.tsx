import * as React from "react";
import type { components } from "@/lib/api/schema";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { useDeleteDepartment } from "./hooks";

type Department = components["schemas"]["QueryDepartmentsResponse"];

interface DeleteDepartmentDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  department: Department | null;
}

export function DeleteDepartmentDialog({
  open,
  onOpenChange,
  department,
}: DeleteDepartmentDialogProps) {
  const deleteDepartment = useDeleteDepartment();
  const [error, setError] = React.useState<string | null>(null);

  const handleDelete = async () => {
    if (!department) return;

    try {
      setError(null);
      await deleteDepartment.mutateAsync({
        params: { path: { id: department.id } },
      });
      onOpenChange(false);
    } catch (err: unknown) {
      // API returns validation error if department has active users
      const errorMessage =
        (err as { detail?: string; message?: string })?.detail ||
        (err as { detail?: string; message?: string })?.message ||
        "Failed to delete department. It may have active users.";
      setError(errorMessage);
    }
  };

  React.useEffect(() => {
    if (!open) {
      setError(null);
    }
  }, [open]);

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete Department</AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to delete the department &ldquo;
            {department?.name}&rdquo;? This action cannot be undone.
            {error && (
              <div className="mt-4 p-3 bg-destructive/10 border border-destructive rounded text-destructive text-sm">
                {error}
              </div>
            )}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={(e: React.MouseEvent) => {
              e.preventDefault();
              handleDelete();
            }}
            disabled={deleteDepartment.isPending}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            {deleteDepartment.isPending ? "Deleting..." : "Delete"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
