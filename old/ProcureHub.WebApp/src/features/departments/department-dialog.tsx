import * as React from "react";
import type { components } from "@/lib/api/schema";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { FormErrorAlert } from "@/components/form-error-alert";
import {
  useProblemDetails,
  setFormFieldErrors,
} from "@/hooks/use-problem-details";
import { useCreateDepartment, useUpdateDepartment } from "./hooks";
import { departmentFormSchema, type DepartmentFormData } from "./types";

type Department = components["schemas"]["QueryDepartmentsResponse"];

interface DepartmentDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  department?: Department | null;
}

export function DepartmentDialog({
  open,
  onOpenChange,
  department,
}: DepartmentDialogProps) {
  const createDepartment = useCreateDepartment();
  const updateDepartment = useUpdateDepartment();
  const isEditing = !!department;

  const form = useForm<DepartmentFormData>({
    resolver: zodResolver(departmentFormSchema),
    defaultValues: isEditing
      ? { id: department.id, name: department.name }
      : { name: "" },
  });

  // Reset form when department data changes
  React.useEffect(() => {
    if (department && isEditing) {
      form.reset({ id: department.id, name: department.name });
    } else {
      form.reset({ name: "" });
    }
  }, [department, isEditing, form]);

  // Extract ProblemDetails from mutation errors
  const activeMutation = isEditing ? updateDepartment : createDepartment;
  const problem = useProblemDetails(activeMutation.error);

  // Sync field errors to form
  React.useEffect(() => {
    if (problem.hasFieldErrors) {
      setFormFieldErrors(form, problem.fieldErrors);
    }
  }, [problem.fieldErrors, problem.hasFieldErrors, form]);

  // Reset mutation state only when dialog opens
  const prevOpenRef = React.useRef(false);
  React.useEffect(() => {
    if (open && !prevOpenRef.current) {
      createDepartment.reset();
      updateDepartment.reset();
    }
    prevOpenRef.current = open;
  }, [open, createDepartment, updateDepartment]);

  const onSubmit = async (data: DepartmentFormData) => {
    try {
      if (isEditing && data.id) {
        await updateDepartment.mutateAsync({
          params: { path: { id: data.id } },
          body: { id: data.id, name: data.name },
        });
        toast.success("Department updated successfully");
      } else {
        await createDepartment.mutateAsync({
          body: { name: data.name },
        });
        toast.success("Department created successfully");
        form.reset();
      }
      onOpenChange(false);
    } catch {
      // Error handled via useProblemDetails
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {isEditing ? "Edit Department" : "Create Department"}
          </DialogTitle>
          <DialogDescription>
            {isEditing
              ? "Update the department name."
              : "Create a new department in the system."}
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormErrorAlert message={problem.summary} />

            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Name</FormLabel>
                  <FormControl>
                    <Input placeholder="Department" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={activeMutation.isPending}>
                {activeMutation.isPending
                  ? isEditing
                    ? "Updating..."
                    : "Creating..."
                  : isEditing
                    ? "Update"
                    : "Create"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
