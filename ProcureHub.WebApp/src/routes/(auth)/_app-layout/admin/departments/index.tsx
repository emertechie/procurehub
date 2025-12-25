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
import { Button } from "@/components/ui/button";

export const Route = createFileRoute("/(auth)/_app-layout/admin/departments/")({
  component: AdminDepartmentsPage,
});

function AdminDepartmentsPage() {
  const { data, isPending, isError, error } = api.useQuery(
    "get",
    "/departments",
  );

  const departments = data?.data ?? [];

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Department Management
          </h1>
          <p className="text-muted-foreground">
            Manage organizational departments
          </p>
        </div>
        <Button>Create Department</Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Departments</CardTitle>
          <CardDescription>
            {isPending
              ? "Loading departments..."
              : `${departments.length} department${departments.length !== 1 ? "s" : ""}`}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isPending && (
            <div className="text-center py-8 text-muted-foreground">
              Loading departments...
            </div>
          )}

          {isError && (
            <div className="text-center py-8 text-destructive">
              Error loading departments: {error?.detail || "Unknown error"}
            </div>
          )}

          {data && (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Department Name</TableHead>
                  <TableHead>ID</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {departments.length === 0 ? (
                  <TableRow>
                    <TableCell
                      colSpan={3}
                      className="text-center text-muted-foreground"
                    >
                      No departments found. Create one to get started.
                    </TableCell>
                  </TableRow>
                ) : (
                  departments.map((department) => (
                    <TableRow key={department.id}>
                      <TableCell className="font-medium">
                        {department.name}
                      </TableCell>
                      <TableCell className="font-mono text-xs text-muted-foreground">
                        {department.id}
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button variant="ghost" size="sm">
                            Edit
                          </Button>
                          <Button variant="ghost" size="sm">
                            Delete
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
