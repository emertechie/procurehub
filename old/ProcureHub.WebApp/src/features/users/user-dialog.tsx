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
import { Form } from "@/components/ui/form";
import { Button } from "@/components/ui/button";
import { FormErrorAlert } from "@/components/form-error-alert";
import {
  useProblemDetails,
  setFormFieldErrors,
} from "@/hooks/use-problem-details";
import { useCreateUser, useUpdateUser } from "./hooks";
import { UserFormFields } from "./user-form-fields";
import { userFormSchema, type UserFormData } from "./types";

type User = components["schemas"]["QueryUsersResponse"];

interface UserDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user?: User | null;
}

export function UserDialog({ open, onOpenChange, user }: UserDialogProps) {
  const createUser = useCreateUser();
  const updateUser = useUpdateUser();
  const isEditing = !!user;

  const form = useForm<UserFormData>({
    resolver: zodResolver(userFormSchema),
    defaultValues: isEditing
      ? {
          id: user.id,
          email: user.email,
          firstName: user.firstName,
          lastName: user.lastName,
        }
      : {
          email: "",
          password: "",
          firstName: "",
          lastName: "",
        },
  });

  // Reset form when user data changes
  React.useEffect(() => {
    if (user && isEditing) {
      form.reset({
        id: user.id,
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
      });
    } else {
      form.reset({
        email: "",
        password: "",
        firstName: "",
        lastName: "",
      });
    }
  }, [user, isEditing, form]);

  // Extract ProblemDetails from mutation errors
  const activeMutation = isEditing ? updateUser : createUser;
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
      createUser.reset();
      updateUser.reset();
    }
    prevOpenRef.current = open;
  }, [open, createUser, updateUser]);

  const onSubmit = async (data: UserFormData) => {
    try {
      if (isEditing && data.id) {
        await updateUser.mutateAsync({
          params: { path: { id: data.id } },
          body: {
            id: data.id,
            email: data.email,
            firstName: data.firstName,
            lastName: data.lastName,
          },
        });
        toast.success("User updated successfully");
      } else if (data.password) {
        await createUser.mutateAsync({
          body: {
            email: data.email,
            password: data.password,
            firstName: data.firstName,
            lastName: data.lastName,
          },
        });
        toast.success("User created successfully");
        form.reset();
      }
      onOpenChange(false);
    } catch {
      // Error handled via useProblemDetails
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-125">
        <DialogHeader>
          <DialogTitle>{isEditing ? "Edit User" : "Create User"}</DialogTitle>
          <DialogDescription>
            {isEditing
              ? "Update user account details"
              : "Create a new user account"}
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormErrorAlert message={problem.summary} />

            <UserFormFields form={form} isEditing={isEditing} />

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
                    ? "Saving..."
                    : "Creating..."
                  : isEditing
                    ? "Save Changes"
                    : "Create User"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
