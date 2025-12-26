import * as React from "react";
import type { components } from "@/lib/api/schema";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Switch } from "@/components/ui/switch";
import { useEnableUser, useDisableUser } from "./hooks";

type User = components["schemas"]["QueryUsersResponse"];

interface UserTableProps {
  users: User[];
  onEditUser: (user: User) => void;
  onManageRoles: (user: User) => void;
  onAssignDepartment: (user: User) => void;
}

export function UserTable({
  users,
  onEditUser,
  onManageRoles,
  onAssignDepartment,
}: UserTableProps) {
  const enableUser = useEnableUser();
  const disableUser = useDisableUser();

  const handleToggleEnabled = async (user: User) => {
    if (user.enabledAt) {
      await disableUser.mutateAsync({
        params: { path: { id: user.id } },
      });
    } else {
      await enableUser.mutateAsync({
        params: { path: { id: user.id } },
      });
    }
  };

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Name</TableHead>
          <TableHead>Email</TableHead>
          <TableHead>Department</TableHead>
          <TableHead>Roles</TableHead>
          <TableHead>Status</TableHead>
          <TableHead className="text-right">Actions</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {users.length === 0 ? (
          <TableRow>
            <TableCell
              colSpan={6}
              className="text-center text-muted-foreground"
            >
              No users found
            </TableCell>
          </TableRow>
        ) : (
          users.map((user) => (
            <TableRow key={user.id}>
              <TableCell className="font-medium">
                {user.firstName} {user.lastName}
              </TableCell>
              <TableCell>{user.email}</TableCell>
              <TableCell>
                {user.department ? (
                  <Badge variant="outline">{user.department.name}</Badge>
                ) : (
                  <Button
                    variant="link"
                    size="sm"
                    className="h-auto p-0 text-xs"
                    onClick={() => onAssignDepartment(user)}
                  >
                    Assign department
                  </Button>
                )}
              </TableCell>
              <TableCell>
                <div className="flex gap-1 flex-wrap items-center">
                  {user.roles.length > 0 ? (
                    user.roles.map((role) => (
                      <Badge key={role} variant="secondary">
                        {role}
                      </Badge>
                    ))
                  ) : (
                    <span className="text-xs text-muted-foreground">
                      No roles
                    </span>
                  )}
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-6 px-2 text-xs"
                    onClick={() => onManageRoles(user)}
                  >
                    Manage
                  </Button>
                </div>
              </TableCell>
              <TableCell>
                <div className="flex items-center gap-2">
                  <Switch
                    checked={!!user.enabledAt}
                    onCheckedChange={() => handleToggleEnabled(user)}
                    disabled={enableUser.isPending || disableUser.isPending}
                  />
                  <span className="text-sm">
                    {user.enabledAt ? "Active" : "Disabled"}
                  </span>
                </div>
              </TableCell>
              <TableCell className="text-right">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => onEditUser(user)}
                >
                  Edit
                </Button>
              </TableCell>
            </TableRow>
          ))
        )}
      </TableBody>
    </Table>
  );
}
