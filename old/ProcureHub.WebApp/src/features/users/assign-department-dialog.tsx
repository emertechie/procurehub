import * as React from "react";
import type { components } from "@/lib/api/schema";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import { useAssignUserToDepartment } from "./hooks";
import { useDepartments } from "../departments";

type User = components["schemas"]["QueryUsersResponse"];

interface AssignDepartmentDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user: User | null;
}

export function AssignDepartmentDialog({
  open,
  onOpenChange,
  user,
}: AssignDepartmentDialogProps) {
  const { data: departmentsData } = useDepartments();
  const assignDepartment = useAssignUserToDepartment();
  const [selectedDepartmentId, setSelectedDepartmentId] = React.useState<
    string | null
  >(null);

  React.useEffect(() => {
    if (user?.department) {
      setSelectedDepartmentId(user.department.id);
    } else {
      setSelectedDepartmentId(null);
    }
  }, [user]);

  if (!user) return null;

  const handleAssign = async () => {
    try {
      await assignDepartment.mutateAsync({
        params: {
          path: { id: user.id },
        },
        body: {
          id: user.id,
          departmentId: selectedDepartmentId,
        },
      });
      toast.success(
        selectedDepartmentId
          ? "Department assigned successfully"
          : "Department unassigned successfully",
      );
      onOpenChange(false);
    } catch (error) {
      console.error("Failed to assign department:", error);
    }
  };

  const departments = departmentsData?.data || [];

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-125">
        <DialogHeader>
          <DialogTitle>Assign Department</DialogTitle>
          <DialogDescription>
            Assign {user.firstName} {user.lastName} to a department
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div className="space-y-2">
            <label className="text-sm font-medium">Department</label>
            <Select
              value={selectedDepartmentId || "none"}
              onValueChange={(value) =>
                setSelectedDepartmentId(value === "none" ? null : value)
              }
            >
              <SelectTrigger>
                <SelectValue placeholder="Select a department" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="none">None (Unassign)</SelectItem>
                {departments.map((dept) => (
                  <SelectItem key={dept.id} value={dept.id}>
                    {dept.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button onClick={handleAssign} disabled={assignDepartment.isPending}>
            {assignDepartment.isPending ? "Assigning..." : "Assign"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
