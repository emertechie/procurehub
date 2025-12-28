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
import { Button } from "@/components/ui/button";
import {
  DepartmentDialog,
  DeleteDepartmentDialog,
  DepartmentTable,
  useDepartments,
} from "@/features/departments";

type Department = components["schemas"]["QueryDepartmentsResponse"];

export const Route = createFileRoute("/(auth)/_app-layout/admin/departments/")({
  component: AdminDepartmentsPage,
});

function AdminDepartmentsPage() {
  const { data, isPending, isError, error } = useDepartments();
  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = React.useState(false);
  const [selectedDepartment, setSelectedDepartment] =
    React.useState<Department | null>(null);

  const departments = data?.data ?? [];

  const handleCreate = () => {
    setSelectedDepartment(null);
    setDialogOpen(true);
  };

  const handleEdit = (department: Department) => {
    setSelectedDepartment(department);
    setDialogOpen(true);
  };

  const handleDelete = (department: Department) => {
    setSelectedDepartment(department);
    setDeleteDialogOpen(true);
  };

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
        <Button onClick={handleCreate}>Create Department</Button>
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
            <DepartmentTable
              departments={departments}
              onEdit={handleEdit}
              onDelete={handleDelete}
            />
          )}
        </CardContent>
      </Card>

      <DepartmentDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        department={selectedDepartment}
      />

      <DeleteDepartmentDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        department={selectedDepartment}
      />
    </div>
  );
}
