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
import { Button } from "@/components/ui/button";

type Department = components["schemas"]["QueryDepartmentsResponse"];

interface DepartmentTableProps {
  departments: Department[];
  onEdit: (department: Department) => void;
  onDelete: (department: Department) => void;
}

export function DepartmentTable({
  departments,
  onEdit,
  onDelete,
}: DepartmentTableProps) {
  return (
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
              <TableCell className="font-medium">{department.name}</TableCell>
              <TableCell className="font-mono text-xs text-muted-foreground">
                {department.id}
              </TableCell>
              <TableCell className="text-right">
                <div className="flex justify-end gap-2">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => onEdit(department)}
                  >
                    Edit
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => onDelete(department)}
                  >
                    Delete
                  </Button>
                </div>
              </TableCell>
            </TableRow>
          ))
        )}
      </TableBody>
    </Table>
  );
}
