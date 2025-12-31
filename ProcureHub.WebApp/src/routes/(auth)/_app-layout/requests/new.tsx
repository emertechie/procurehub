import * as React from "react";
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { toast } from "sonner";

import {
  PurchaseRequestForm,
  useCreatePurchaseRequest,
  useSubmitPurchaseRequest,
  type CreatePurchaseRequestFormData,
} from "@/features/purchase-requests";
import { useCategories } from "@/features/categories";
import { useDepartments } from "@/features/departments";

export const Route = createFileRoute("/(auth)/_app-layout/requests/new")({
  component: NewRequestPage,
});

function NewRequestPage() {
  const navigate = useNavigate();
  const createMutation = useCreatePurchaseRequest();
  const submitMutation = useSubmitPurchaseRequest();

  const { data: categoriesData, isPending: isCategoriesLoading } =
    useCategories();
  const { data: departmentsData, isPending: isDepartmentsLoading } =
    useDepartments();

  const categories = categoriesData?.data ?? [];
  const departments = departmentsData?.data ?? [];

  const handleSaveAsDraft = async (data: CreatePurchaseRequestFormData) => {
    try {
      await createMutation.mutateAsync({
        body: {
          title: data.title,
          description: data.description || null,
          estimatedAmount: data.estimatedAmount,
          businessJustification: data.businessJustification || null,
          categoryId: data.categoryId,
          departmentId: data.departmentId,
          requesterUserId: "", // Server will override with authenticated user
        },
      });
      toast.success("Purchase request saved as draft");
      navigate({ to: "/requests" });
    } catch {
      // Error handled via form
    }
  };

  const handleSubmitForApproval = async (
    data: CreatePurchaseRequestFormData,
  ) => {
    try {
      // First create the request
      const result = await createMutation.mutateAsync({
        body: {
          title: data.title,
          description: data.description || null,
          estimatedAmount: data.estimatedAmount,
          businessJustification: data.businessJustification || null,
          categoryId: data.categoryId,
          departmentId: data.departmentId,
          requesterUserId: "", // Server will override with authenticated user
        },
      });

      // Then submit it for approval
      const newId = result.id;
      if (newId) {
        await submitMutation.mutateAsync({
          params: { path: { id: newId } },
        });
        toast.success("Purchase request submitted for approval");
        navigate({ to: "/requests" });
      }
    } catch {
      // Error handled via form
    }
  };

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">
          New Purchase Request
        </h1>
        <p className="text-muted-foreground text-sm">
          Create a new purchase request for goods or services
        </p>
      </div>

      <PurchaseRequestForm
        categories={categories}
        departments={departments}
        isCategoriesLoading={isCategoriesLoading}
        isDepartmentsLoading={isDepartmentsLoading}
        onSaveAsDraft={handleSaveAsDraft}
        onSubmitForApproval={handleSubmitForApproval}
        isSaving={createMutation.isPending && !submitMutation.isPending}
        isSubmitting={submitMutation.isPending}
        saveError={createMutation.error}
        submitError={submitMutation.error || createMutation.error}
      />
    </div>
  );
}
