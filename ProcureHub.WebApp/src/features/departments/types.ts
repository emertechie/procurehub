import * as z from "zod";

export const createDepartmentSchema = z.object({
  name: z.string().min(1, "Department name is required"),
});

export const updateDepartmentSchema = z.object({
  id: z.string(),
  name: z.string().min(1, "Department name is required"),
});

// Combined schema for unified form handling
export const departmentFormSchema = z.object({
  id: z.string().optional(),
  name: z.string().min(1, "Department name is required"),
});

export type CreateDepartmentFormData = z.infer<typeof createDepartmentSchema>;
export type UpdateDepartmentFormData = z.infer<typeof updateDepartmentSchema>;
export type DepartmentFormData = z.infer<typeof departmentFormSchema>;
