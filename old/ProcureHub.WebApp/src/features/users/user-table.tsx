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
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  MoreHorizontal,
  Pencil,
  Shield,
  UserX,
  UserCheck,
  Building2,
} from "lucide-react";
import { useEnableUser, useDisableUser } from "./hooks";
import { getRoleBadgeClasses } from "./role-badge-utils";

type User = components["schemas"]["QueryUsersResponse"];

interface UserTableProps {
  users: User[];
  onEditUser: (user: User) => void;
  onManageRoles: (user: User) => void;
  onAssignDepartment: (user: User) => void;
}

function getInitials(firstName: string, lastName: string): string {
  return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
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
          <TableHead>User</TableHead>
          <TableHead>Role</TableHead>
          <TableHead>Department</TableHead>
          <TableHead>Status</TableHead>
          <TableHead className="w-12"></TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {users.length === 0 ? (
          <TableRow>
            <TableCell
              colSpan={5}
              className="text-center text-muted-foreground"
            >
              No users found
            </TableCell>
          </TableRow>
        ) : (
          users.map((user) => (
            <TableRow key={user.id}>
              <TableCell>
                <div className="flex items-center gap-3">
                  <Avatar className="size-10">
                    <AvatarFallback className="bg-gray-100 text-gray-600 text-sm font-medium">
                      {getInitials(user.firstName, user.lastName)}
                    </AvatarFallback>
                  </Avatar>
                  <div className="flex flex-col">
                    <span className="font-medium">
                      {user.firstName} {user.lastName}
                    </span>
                    <span className="text-sm text-muted-foreground">
                      {user.email}
                    </span>
                  </div>
                </div>
              </TableCell>
              <TableCell>
                <div className="flex gap-1.5 flex-wrap">
                  {user.roles.length > 0 ? (
                    user.roles.map((role) => (
                      <Badge
                        key={role}
                        variant="outline"
                        className={getRoleBadgeClasses(role)}
                      >
                        {role}
                      </Badge>
                    ))
                  ) : (
                    <span className="text-sm text-muted-foreground">—</span>
                  )}
                </div>
              </TableCell>
              <TableCell>
                <span className="text-muted-foreground">
                  {user.department?.name ?? "—"}
                </span>
              </TableCell>
              <TableCell>
                <Badge
                  variant="outline"
                  className={
                    user.enabledAt
                      ? "border-green-300 bg-green-50 text-green-700"
                      : "border-gray-300 bg-gray-100 text-gray-500"
                  }
                >
                  {user.enabledAt ? "Active" : "Disabled"}
                </Badge>
              </TableCell>
              <TableCell>
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" size="icon" className="size-8">
                      <MoreHorizontal className="size-4" />
                      <span className="sr-only">Open menu</span>
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end">
                    <DropdownMenuItem onClick={() => onEditUser(user)}>
                      <Pencil className="size-4" />
                      Edit User
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => onManageRoles(user)}>
                      <Shield className="size-4" />
                      Change Role
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => onAssignDepartment(user)}>
                      <Building2 className="size-4" />
                      Change Department
                    </DropdownMenuItem>
                    <DropdownMenuItem
                      variant={user.enabledAt ? "destructive" : "default"}
                      onClick={() => handleToggleEnabled(user)}
                      disabled={enableUser.isPending || disableUser.isPending}
                    >
                      {user.enabledAt ? (
                        <>
                          <UserX className="size-4" />
                          Deactivate
                        </>
                      ) : (
                        <>
                          <UserCheck className="size-4" />
                          Activate
                        </>
                      )}
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </TableCell>
            </TableRow>
          ))
        )}
      </TableBody>
    </Table>
  );
}
