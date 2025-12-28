import * as React from "react";
import type { components } from "@/lib/api/schema";
import { api } from "@/lib/api/client";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { toast } from "sonner";
import { useAssignRole, useRemoveRole, useUser } from "./hooks";

type User = components["schemas"]["QueryUsersResponse"];

interface ManageRolesDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user: User | null;
}

export function ManageRolesDialog({
  open,
  onOpenChange,
  user,
}: ManageRolesDialogProps) {
  const { data: rolesData } = api.useQuery("get", "/roles");
  const { data: userData } = useUser(user?.id || "", open);
  const assignRole = useAssignRole();
  const removeRole = useRemoveRole();

  if (!user) return null;

  const currentUser = userData?.data || user;
  const availableRoles = rolesData?.data || [];
  const currentRoles = new Set(currentUser.roles);

  const handleToggleRole = async (roleName: string) => {
    const role = availableRoles.find((r) => r.name === roleName);
    if (!role) return;

    const isCurrentlyAssigned = currentRoles.has(roleName);

    try {
      if (isCurrentlyAssigned) {
        await removeRole.mutateAsync({
          params: {
            path: {
              userId: user.id,
              roleId: role.id,
            },
          },
        });
      } else {
        await assignRole.mutateAsync({
          params: {
            path: {
              userId: user.id,
            },
          },
          body: {
            userId: user.id,
            roleId: role.id,
          },
        });
      }
      toast.success("Role updated successfully");
    } catch (error) {
      console.error("Failed to toggle role:", error);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-125">
        <DialogHeader>
          <DialogTitle>Manage Roles</DialogTitle>
          <DialogDescription>
            Assign or remove roles for {currentUser.firstName}{" "}
            {currentUser.lastName}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div className="space-y-2">
            <div className="text-sm font-medium">Available Roles</div>
            {availableRoles.map((role) => (
              <div
                key={role.id}
                className="flex items-center space-x-2 rounded-md border p-3"
              >
                <Checkbox
                  id={role.id}
                  checked={currentRoles.has(role.name)}
                  onCheckedChange={() => handleToggleRole(role.name)}
                  disabled={assignRole.isPending || removeRole.isPending}
                />
                <label
                  htmlFor={role.id}
                  className="flex-1 cursor-pointer text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
                >
                  {role.name}
                </label>
              </div>
            ))}
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Close
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
