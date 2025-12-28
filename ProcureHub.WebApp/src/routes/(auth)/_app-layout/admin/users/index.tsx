import * as React from "react";
import { createFileRoute } from "@tanstack/react-router";
import type { components } from "@/lib/api/schema";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import {
  UserTable,
  UserDialog,
  ManageRolesDialog,
  AssignDepartmentDialog,
  useUsers,
} from "@/features/users";

type User = components["schemas"]["QueryUsersResponse"];

export const Route = createFileRoute("/(auth)/_app-layout/admin/users/")({
  component: AdminUsersPage,
});

function AdminUsersPage() {
  const [searchEmail, setSearchEmail] = React.useState("");
  const [page, setPage] = React.useState(1);
  const pageSize = 20;

  const [userDialogOpen, setUserDialogOpen] = React.useState(false);
  const [rolesDialogOpen, setRolesDialogOpen] = React.useState(false);
  const [departmentDialogOpen, setDepartmentDialogOpen] = React.useState(false);
  const [selectedUser, setSelectedUser] = React.useState<User | null>(null);

  const { data, isPending, isError, error } = useUsers(
    searchEmail,
    page,
    pageSize,
  );

  const handleCreateUser = () => {
    setSelectedUser(null);
    setUserDialogOpen(true);
  };

  const handleEditUser = (user: User) => {
    setSelectedUser(user);
    setUserDialogOpen(true);
  };

  const handleManageRoles = (user: User) => {
    setSelectedUser(user);
    setRolesDialogOpen(true);
  };

  const handleAssignDepartment = (user: User) => {
    setSelectedUser(user);
    setDepartmentDialogOpen(true);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Users</h1>
          <p className="text-muted-foreground text-sm">
            Manage user accounts, roles, and permissions
          </p>
        </div>
        <Button onClick={handleCreateUser}>Create User</Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Search Users</CardTitle>
          <CardDescription>
            Filter users by email address or view all users
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex gap-2">
            <Input
              placeholder="Search by email..."
              value={searchEmail}
              onChange={(e) => setSearchEmail(e.target.value)}
              className="max-w-sm"
            />
            <Button
              variant="outline"
              onClick={() => {
                setSearchEmail("");
                setPage(1);
              }}
            >
              Clear
            </Button>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Users</CardTitle>
          <CardDescription>
            {data?.pagination
              ? `Showing ${data.data.length} of ${data.pagination.totalCount} users`
              : "Loading users..."}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isPending && (
            <div className="text-center py-8 text-muted-foreground">
              Loading users...
            </div>
          )}

          {isError && (
            <div className="text-center py-8 text-destructive">
              Error loading users: {error?.detail || "Unknown error"}
            </div>
          )}

          {data && (
            <>
              <UserTable
                users={data.data}
                onEditUser={handleEditUser}
                onManageRoles={handleManageRoles}
                onAssignDepartment={handleAssignDepartment}
              />

              {data.pagination && (
                <div className="flex items-center justify-between mt-4">
                  <div className="text-sm text-muted-foreground">
                    Page {data.pagination.page} of{" "}
                    {Math.ceil(
                      Number(data.pagination.totalCount) /
                        Number(data.pagination.pageSize),
                    )}
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setPage((p) => Math.max(1, p - 1))}
                      disabled={page === 1}
                    >
                      Previous
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setPage((p) => p + 1)}
                      disabled={
                        page >=
                        Math.ceil(
                          Number(data.pagination.totalCount) /
                            Number(data.pagination.pageSize),
                        )
                      }
                    >
                      Next
                    </Button>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      <UserDialog
        open={userDialogOpen}
        onOpenChange={setUserDialogOpen}
        user={selectedUser}
      />
      <ManageRolesDialog
        open={rolesDialogOpen}
        onOpenChange={setRolesDialogOpen}
        user={selectedUser}
      />
      <AssignDepartmentDialog
        open={departmentDialogOpen}
        onOpenChange={setDepartmentDialogOpen}
        user={selectedUser}
      />
    </div>
  );
}
