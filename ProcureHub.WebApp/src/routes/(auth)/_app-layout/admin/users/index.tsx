import * as React from "react";
import { createFileRoute } from "@tanstack/react-router";
import { api } from "@/lib/api/client";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

export const Route = createFileRoute("/(auth)/_app-layout/admin/users/")({
  component: AdminUsersPage,
});

function AdminUsersPage() {
  const [searchEmail, setSearchEmail] = React.useState("");
  const [page, setPage] = React.useState(1);
  const pageSize = 20;

  const { data, isPending, isError, error } = api.useQuery("get", "/users", {
    params: {
      query: {
        Email: searchEmail || undefined,
        Page: page,
        PageSize: pageSize,
      },
    },
  });

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">User Management</h1>
          <p className="text-muted-foreground">
            Manage user accounts, roles, and permissions
          </p>
        </div>
        <Button>Create User</Button>
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
                  {data.data.length === 0 ? (
                    <TableRow>
                      <TableCell
                        colSpan={6}
                        className="text-center text-muted-foreground"
                      >
                        No users found
                      </TableCell>
                    </TableRow>
                  ) : (
                    data.data.map((user) => (
                      <TableRow key={user.id}>
                        <TableCell className="font-medium">
                          {user.firstName} {user.lastName}
                        </TableCell>
                        <TableCell>{user.email}</TableCell>
                        <TableCell>
                          {user.department ? (
                            <Badge variant="outline">
                              {user.department.name}
                            </Badge>
                          ) : (
                            <span className="text-xs text-muted-foreground">
                              Not assigned
                            </span>
                          )}
                        </TableCell>
                        <TableCell>
                          <div className="flex gap-1 flex-wrap">
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
                          </div>
                        </TableCell>
                        <TableCell>
                          {user.enabledAt ? (
                            <Badge variant="default">Active</Badge>
                          ) : (
                            <Badge variant="destructive">Disabled</Badge>
                          )}
                        </TableCell>
                        <TableCell className="text-right">
                          <Button variant="ghost" size="sm">
                            Edit
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>

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
    </div>
  );
}
