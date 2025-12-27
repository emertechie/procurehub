import * as React from "react";
import type { components } from "@/lib/api/schema";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
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
import { useCreateDepartment, useUpdateDepartment } from "./hooks";
import {
  createDepartmentSchema,
  updateDepartmentSchema,
  type CreateDepartmentFormData,
  type UpdateDepartmentFormData,
} from "./types";

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

  const createForm = useForm<CreateDepartmentFormData>({
    resolver: zodResolver(createDepartmentSchema),
    defaultValues: {
      name: "",
    },
  });

  const editForm = useForm<UpdateDepartmentFormData>({
    resolver: zodResolver(updateDepartmentSchema),
    defaultValues: {
      id: department?.id || "",
      name: department?.name || "",
    },
  });

  React.useEffect(() => {
    if (department && isEditing) {
      editForm.reset({
        id: department.id,
        name: department.name,
      });
    } else {
      createForm.reset({
        name: "",
      });
    }
  }, [department, isEditing, createForm, editForm]);

  const onCreateSubmit = async (data: CreateDepartmentFormData) => {
    try {
      await createDepartment.mutateAsync({
        body: data,
      });
      onOpenChange(false);
      createForm.reset();
    } catch (error) {
      console.error("Failed to create department:", error);
    }
  };

  const onUpdateSubmit = async (data: UpdateDepartmentFormData) => {
    try {
      await updateDepartment.mutateAsync({
        params: { path: { id: data.id } },
        body: { id: data.id, name: data.name },
      });
      onOpenChange(false);
    } catch (error) {
      console.error("Failed to update department:", error);
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

        {isEditing ? (
          <Form {...editForm}>
            <form
              onSubmit={editForm.handleSubmit(onUpdateSubmit)}
              className="space-y-4"
            >
              <FormField
                control={editForm.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Department Name</FormLabel>
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
                <Button type="submit" disabled={updateDepartment.isPending}>
                  {updateDepartment.isPending ? "Updating..." : "Update"}
                </Button>
              </DialogFooter>
            </form>
          </Form>
        ) : (
          <Form {...createForm}>
            <form
              onSubmit={createForm.handleSubmit(onCreateSubmit)}
              className="space-y-4"
            >
              <FormField
                control={createForm.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Department Name</FormLabel>
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
                <Button type="submit" disabled={createDepartment.isPending}>
                  {createDepartment.isPending ? "Creating..." : "Create"}
                </Button>
              </DialogFooter>
            </form>
          </Form>
        )}
      </DialogContent>
    </Dialog>
  );
}
